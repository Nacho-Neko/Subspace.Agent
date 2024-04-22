using Subspace.Agent.Core;
using Subspace.Agent.Core.EventBus;
using System.Reflection;

namespace Subspace.Agent.EventBus
{
    public delegate Task EventBusHandler<TEventArgs>(RpcClient sender, TEventArgs e);
    public class EventBus : IBusControl
    {
        private readonly Dictionary<Type, Delegate> _eventHandlers = new Dictionary<Type, Delegate>();
        public void Subscribe<TEventArgs>(EventBusHandler<TEventArgs> handler)
        {
            Type eventType = typeof(TEventArgs);
            if (_eventHandlers.ContainsKey(eventType))
            {
                _eventHandlers[eventType] = Delegate.Combine(_eventHandlers[eventType], handler);
            }
            else
            {
                _eventHandlers[eventType] = handler;
            }
        }
        public void Unsubscribe<TEventArgs>(EventBusHandler<TEventArgs> handler)
        {
            Type eventType = typeof(TEventArgs);
            if (_eventHandlers.ContainsKey(eventType))
            {
                _eventHandlers[eventType] = Delegate.Remove(_eventHandlers[eventType], handler);
            }
        }
        public void Publish<TEventArgs>(RpcClient sender, TEventArgs e)
        {
            Type eventType = typeof(TEventArgs);
            if (_eventHandlers.ContainsKey(eventType))
            {
                EventBusHandler<TEventArgs> handler = (EventBusHandler<TEventArgs>)_eventHandlers[eventType];
                _ = Task.Run(async () =>
                {
                    await handler?.Invoke(sender, e);
                });
            }
        }
        public void Start()
        {
            Assembly assembly = typeof(IBusControl).Assembly;
            var typesWithConsumers = new List<Type>();
            foreach (Type type in assembly.GetTypes())
            {
                if (type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IConsumer<>)))
                {
                    typesWithConsumers.Add(type);
                }
            }

            foreach (var type in typesWithConsumers)
            {
                Type consumerInterface = type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IConsumer<>));
                object instance = Activator.CreateInstance(type);
                MethodInfo consumeMethod = type.GetMethod("ConsumeAsync");
                if (consumeMethod != null)
                {
                    Type consumerType = consumerInterface.GetGenericArguments()[0]; // 获取泛型参数
                    Type delegateType = typeof(EventBusHandler<>).MakeGenericType(consumerType);

                    Delegate @delegate = Delegate.CreateDelegate(delegateType, instance, consumeMethod);
                    if (_eventHandlers.TryGetValue(consumerType, out Delegate value))
                    {
                        value = Delegate.Combine(value, @delegate);
                    }
                    else
                    {
                        _eventHandlers.Add(consumerType, @delegate);
                    }
                }
            }
        }
    }
}
