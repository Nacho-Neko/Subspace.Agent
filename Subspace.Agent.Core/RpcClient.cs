using Autofac;
using Microsoft.Extensions.Logging;
using StreamJsonRpc;
using Subspace.Agent.Core.Model;
using Subspace.Agent.Core.Model.Event;
using Subspace.Agent.Core.Model.Method;
using System.Diagnostics;
using System.Reflection;

namespace Subspace.Agent.Core
{
    public class RpcClient : JsonRpc, IDisposable
    {
        public delegate void EventBusHandler(RpcClient sender, ILifetimeScope e);
        public event EventBusHandler? onDisconnected;

        public readonly ILogger logger;
        private readonly ILifetimeScope lifetimeScope;
        private readonly WebSocketMessageHandler webSocketMessageHandler;

#nullable disable
        /// <summary>
        /// 事件
        /// </summary>
        public ArchivedSegmentHeaderEven archivedSegmentHeaderEven;
        public NodeSyncStatusChangeEven nodeSyncStatusChangeEven;
        public RewardSigningEven rewardSigningEven;
        public SlotInfoEven slotInfoEven;

        /// <summary>
        /// 方法
        /// </summary>
        public AcknowledgeArchivedSegmentHeaderMethod acknowledgeArchivedSegment;
        public FarmerInfoMethod getFarmerAppInfo;
        public LastSegmentHeadersMethod lastSegmentHeaders;
        public PieceMethod pieceMethod;
        public SegmentHeadersMethod segmentHeaders;
        public SubmitRewardSignatureMethod submitRewardSignature;
        public SubmitSolutionResponseMethod submitSolution;
#nullable enable
#nullable disable
        public double Latency = -1;
        public NodeInfo NodeInfo;
#nullable enable
        public bool Available { get { if (nodeSyncStatusChangeEven == null) return false; return nodeSyncStatusChangeEven.Synced; } }
        public RpcClient(ILogger<RpcClient> logger, ILifetimeScope lifetimeScope, WebSocketMessageHandler webSocketMessageHandler) : base(webSocketMessageHandler)
        {
            this.logger = logger;
            this.lifetimeScope = lifetimeScope;
            this.webSocketMessageHandler = webSocketMessageHandler;
            Disconnected += RpcClient_Disconnected;
        }
        private void RpcClient_Disconnected(object? sender, JsonRpcDisconnectedEventArgs e)
        {
            onDisconnected?.Invoke(this, lifetimeScope);
        }
        public async Task StartAsync(NodeInfo nodeInfo)
        {
            archivedSegmentHeaderEven = lifetimeScope.Resolve<ArchivedSegmentHeaderEven>();
            nodeSyncStatusChangeEven = lifetimeScope.Resolve<NodeSyncStatusChangeEven>();
            rewardSigningEven = lifetimeScope.Resolve<RewardSigningEven>();
            slotInfoEven = lifetimeScope.Resolve<SlotInfoEven>();

            acknowledgeArchivedSegment = lifetimeScope.Resolve<AcknowledgeArchivedSegmentHeaderMethod>();
            getFarmerAppInfo = lifetimeScope.Resolve<FarmerInfoMethod>();
            lastSegmentHeaders = lifetimeScope.Resolve<LastSegmentHeadersMethod>();
            pieceMethod = lifetimeScope.Resolve<PieceMethod>();
            segmentHeaders = lifetimeScope.Resolve<SegmentHeadersMethod>();
            submitRewardSignature = lifetimeScope.Resolve<SubmitRewardSignatureMethod>();
            submitSolution = lifetimeScope.Resolve<SubmitSolutionResponseMethod>();
            // TraceSource.Switch.Level = System.Diagnostics.SourceLevels.All;
            // TraceSource.Listeners.Add(new ConsoleTraceListener(logger));

            this.NodeInfo = nodeInfo;
            StartListening();
            string result;
            result = await archivedSegmentHeaderEven.SubscribeAsync();
            Thread.Sleep(100);
            result = await rewardSigningEven.SubscribeAsync();
            Thread.Sleep(100);
            result = await slotInfoEven.SubscribeAsync();
            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                await getFarmerAppInfo.InvokeAsync();
                Latency = stopwatch.Elapsed.TotalMilliseconds;
                stopwatch.Stop();
            }
            catch (Exception)
            {
                logger.LogError($"{nodeInfo.name} 已连接，但是无法返回正常数据!");
                Latency = -1;
            }
        }
    }
}
