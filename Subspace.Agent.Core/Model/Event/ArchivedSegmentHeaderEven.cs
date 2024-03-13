using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Subspace.Agent.Core.EventBus;

namespace Subspace.Agent.Core.Model.Event
{
    /// Archived segment header subscription
    public class ArchivedSegmentHeaderEven
    {
        private readonly ILogger logger;
        private readonly IBusControl busControl;
        private readonly RpcClient rpcClient;
        private string subscribeId;
        public ArchivedSegmentHeaderEven(ILogger<ArchivedSegmentHeaderEven> logger, IBusControl busControl, RpcClient rpcClient)
        {
            this.logger = logger;
            this.busControl = busControl;
            this.rpcClient = rpcClient;
            subscribeId = string.Empty;
            rpcClient.AddLocalRpcMethod("subspace_archived_segment_header", new Action<string, SegmentHeader>(SubspaceSegmentHeader));
        }
        public void SubspaceSegmentHeader(string subscription, SegmentHeader result)
        {
            busControl.Publish(rpcClient, result);
        }
        public async Task<string> SubscribeAsync()
        {
            logger.LogTrace($"Method : subscribeArchivedSegmentHeader");
            string subscribeId = await rpcClient.InvokeAsync<string>("subspace_subscribeArchivedSegmentHeader");
            logger.LogTrace($"Result : {subscribeId}");
            return subscribeId;
        }
        public async Task UnSubscribeAsync()
        {
            logger.LogTrace($"Method : unsubscribeArchivedSegmentHeader");
            JObject jobject = await rpcClient.InvokeAsync<JObject>("subspace_unsubscribeArchivedSegmentHeader", subscribeId);
            logger.LogTrace($"Result : {jobject}");
        }
    }
}
