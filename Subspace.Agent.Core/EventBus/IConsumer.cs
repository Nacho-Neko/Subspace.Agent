namespace Subspace.Agent.Core.EventBus
{
    public interface IConsumer<TEventArgs>
    {
        public Task ConsumeAsync(RpcClient sender, TEventArgs e);
    }
}