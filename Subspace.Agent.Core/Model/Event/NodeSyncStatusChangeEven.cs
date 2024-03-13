using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Subspace.Agent.Core.EventBus;

namespace Subspace.Agent.Core.Model.Event
{
    public class NodeSyncStatusChangeEven
    {

        private readonly ILogger logger;
        private readonly IBusControl busControl;
        private readonly RpcClient rpcClient;
        private string subscribeId;
        private NodeSyncStatus nodeSync = NodeSyncStatus.synced;

        public bool Synced { get { return nodeSync == NodeSyncStatus.synced; } }
        public NodeSyncStatusChangeEven(ILogger<NodeSyncStatusChangeEven> logger, IBusControl busControl, RpcClient rpcClient)
        {
            this.logger = logger;
            this.busControl = busControl;
            this.rpcClient = rpcClient;
            subscribeId = string.Empty;
            rpcClient.AddLocalRpcMethod("subspace_node_sync_status_change", new Action<string, NodeSyncStatus>(SubspaceNodeSync));
        }
        private void SubspaceNodeSync(string subscription, NodeSyncStatus result)
        {
            this.nodeSync = result;
            busControl.Publish(rpcClient, result);
        }
        public async Task<string> SubscribeAsync()
        {
            logger.LogTrace($"Method : subscribeNodeSyncStatusChange");
            string subscribeId = await rpcClient.InvokeAsync<string>("subspace_subscribeNodeSyncStatusChange");
            logger.LogTrace($"Result : {subscribeId}");
            return subscribeId;
        }
        public async Task UnSubscribeAsync()
        {
            logger.LogTrace($"Method : unsubscribeNodeSyncStatusChange");
            JObject jobject = await rpcClient.InvokeAsync<JObject>("subspace_unsubscribeNodeSyncStatusChange", subscribeId);
            logger.LogTrace($"Result : {jobject}");
        }
    }
    [System.Text.Json.Serialization.JsonConverter(typeof(StringEnumConverter))]
    [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
    public enum NodeSyncStatus
    {
        /// Node is fully synced
        synced,
        /// Node is major syncing
        majorSyncing,
    }
}
