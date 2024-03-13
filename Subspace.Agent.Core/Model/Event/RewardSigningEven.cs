using Microsoft.Extensions.Logging;
using Subspace.Agent.Core.EventBus;

namespace Subspace.Agent.Core.Model.Event
{
    public class RewardSigningEven
    {
        private readonly ILogger logger;
        private readonly IBusControl busControl;
        private readonly RpcClient rpcClient;
        private string subscribeId;
        public RewardSigningEven(ILogger<RewardSigningEven> logger, IBusControl busControl, RpcClient rpcClient)
        {
            this.logger = logger;
            this.busControl = busControl;
            this.rpcClient = rpcClient;
            subscribeId = string.Empty;
            rpcClient.AddLocalRpcMethod("subspace_reward_signing", new Action<string, RewardSigningInfo>(SubspaceRewardSigningInfo));
        }
        private void SubspaceRewardSigningInfo(string subscription, RewardSigningInfo result)
        {
            busControl.Publish(rpcClient, result);
        }
        public async Task<string> SubscribeAsync()
        {
            logger.LogTrace($"Method : subscribeRewardSigning");
            string subscribeId = await rpcClient.InvokeAsync<string>("subspace_subscribeRewardSigning");
            logger.LogTrace($"Result : {subscribeId}");
            return subscribeId;
        }
        public async Task UnSubscribeAsync()
        {
            logger.LogTrace($"Method : unsubscribeRewardSigning");
            Task task = await rpcClient.InvokeAsync<Task>("subspace_unsubscribeRewardSigning", subscribeId);
        }
        public class RewardSigningInfo
        {
            /// Hash to be signed.
            public string? hash;
            /// Public key of the plot identity that should create signature.
            public string? publicKey;
        }
    }
}
