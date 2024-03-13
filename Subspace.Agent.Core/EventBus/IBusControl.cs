namespace Subspace.Agent.Core.EventBus
{
    public interface IBusControl
    {
        public void Publish<TEventArgs>(RpcClient sender, TEventArgs e);
        public void Start();
    }
}