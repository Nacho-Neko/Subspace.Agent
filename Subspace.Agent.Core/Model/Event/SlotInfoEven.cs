using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Subspace.Agent.Core.EventBus;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Subspace.Agent.Core.Model.Event
{
	public class SlotInfoEven
	{
		private readonly ILogger logger;
		private readonly IBusControl busControl;
		private readonly RpcClient rpcClient;
		private string subscribeId;
		public SlotInfoEven(ILogger<SlotInfoEven> logger, IBusControl busControl, RpcClient rpcClient)
		{
			this.logger = logger;
			this.busControl = busControl;
			this.rpcClient = rpcClient;
			subscribeId = string.Empty;
			rpcClient.AddLocalRpcMethod("subspace_slot_info", new Action<string, SlotInfo>(SubspaceSlotInfo));
		}
		public void SubspaceSlotInfo(string subscription, SlotInfo result)
		{
			busControl.Publish(rpcClient, result);
		}
		public async Task<string> SubscribeAsync()
		{
			logger.LogTrace($"Method : subscribeSlotInfo");
			string subscribeId = await rpcClient.InvokeAsync<string>("subspace_subscribeSlotInfo");
			logger.LogTrace($"Result : {subscribeId}");
			return subscribeId;
		}
		public async Task UnSubscribeAsync()
		{
			logger.LogTrace($"Method : subscribeSlotInfo");
			JObject jobject = await rpcClient.InvokeAsync<JObject>("subspace_unsubscribeSlotInfo", subscribeId);
			logger.LogTrace($"Result : {jobject}");
		}
		public class SlotInfo
		{
			private Stopwatch stopwatch = new Stopwatch();
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public long ElapsedMilliseconds()
			{
				return stopwatch.ElapsedMilliseconds;
			}
			public void Start()
			{
				stopwatch.Start();
			}

			public void Stop()
			{
				stopwatch.Stop();
			}

			public SlotInfo()
			{

			}
#nullable disable
			public ulong slotNumber { get; set; }
			public UInt16[] globalChallenge { get; set; }
			public ulong solutionRange { get; set; }
			public ulong votingSolutionRange { get; set; }
#nullable enable
		}
	}
}
