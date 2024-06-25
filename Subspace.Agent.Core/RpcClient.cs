using Autofac;
using Microsoft.Extensions.Logging;
using StreamJsonRpc;
using Subspace.Agent.Core.Model;
using Subspace.Agent.Core.Model.Event;
using Subspace.Agent.Core.Model.Method;
using System.Diagnostics;

namespace Subspace.Agent.Core
{
    public class RpcClient : JsonRpc, IDisposable
	{
		public delegate void EventBusHandler(RpcClient sender, ILifetimeScope e);
		public event EventBusHandler? onDisconnected;
		public readonly Stopwatch Stopwatch = new Stopwatch();
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

		public long Interval = 0;
        /// <summary>
        /// 节点获取2次区块之间的间隔
        /// </summary>
        public long Delay { get { return Interval + this.Stopwatch.ElapsedMilliseconds; } }
		public NodeInfo NodeInfo;

		public bool Available { get { if (nodeSyncStatusChangeEven == null) return false; return nodeSyncStatusChangeEven.Synced; } }
		public RpcClient(ILogger<RpcClient> logger, ILifetimeScope lifetimeScope, WebSocketMessageHandler webSocketMessageHandler) : base(webSocketMessageHandler)
		{
			this.logger = logger;
			this.lifetimeScope = lifetimeScope;
			this.webSocketMessageHandler = webSocketMessageHandler;
			Stopwatch.Start();
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
			// TraceSource.Switch.Level = SourceLevels.All;
			// TraceSource.Listeners.Add(new ConsoleTraceListener(logger));
			this.NodeInfo = nodeInfo;
			StartListening();
			string result;
			result = await archivedSegmentHeaderEven.SubscribeAsync();
			Thread.Sleep(100);
			result = await rewardSigningEven.SubscribeAsync();
			Thread.Sleep(100);
			result = await slotInfoEven.SubscribeAsync();
		}
	}
}