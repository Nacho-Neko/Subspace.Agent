using Autofac;
using Autofac.Core;
using Microsoft.Extensions.Logging;
using Subspace.Agent.Core.Model;
using System.Net.WebSockets;

namespace Subspace.Agent.Core
{
	public class ClientPool
	{
		private readonly ILogger logger;
		private readonly ILifetimeScope lifetimeScope;

		List<NodeInfo> reConnectQueues = new List<NodeInfo>();

		private readonly Dictionary<ILifetimeScope, RpcClient> Persistent = new Dictionary<ILifetimeScope, RpcClient>();
		public ClientPool(ILogger<ClientPool> logger, ILifetimeScope lifetimeScope)
		{
			this.logger = logger;
			this.lifetimeScope = lifetimeScope;
		}
		public async Task StartAsync(ConfigModel configModel)
		{
			List<NodeInfo> nodes = configModel.Nodes;
			foreach (var node in nodes)
			{
				var scope = lifetimeScope.BeginLifetimeScope(node.Name);
				NodeInfo nodeInfo = node;
				try
				{
					ClientWebSocket clientWebSocket = scope.Resolve<ClientWebSocket>();
					Uri uri = new Uri(node.Url);
					await clientWebSocket.ConnectAsync(uri, CancellationToken.None);
					RpcClient rpcClient = scope.Resolve<RpcClient>();
					await rpcClient.StartAsync(node);
					Persistent.Add(scope, rpcClient);
					logger.LogInformation($"Connect RpcClient {Persistent.Count} Url : {nodeInfo.Url}");
				}
				catch
				{
					reConnectQueues.Add(nodeInfo);
					scope.Dispose();
					logger.LogError($"Connect Fail RpcClient {Persistent.Count} Url : {nodeInfo.Url}");
				}
			}

			foreach (var rpcPool in Persistent)
			{
				RpcClient rpc = rpcPool.Value;
				rpc.onDisconnected += Rpc_onDisconnected;
			}
			_ = Task.Run(ReConnectAsync).ConfigureAwait(false);
		}

		private void Rpc_onDisconnected(RpcClient rpcClient, ILifetimeScope args)
		{
			if (Persistent.TryGetValue(args, out RpcClient? socketRpc))
			{
				reConnectQueues.Add(rpcClient.NodeInfo);
				logger.LogInformation($"Disconnect RpcClient {Persistent.Count} Id : {socketRpc.NodeInfo.Name} Url : {socketRpc.NodeInfo.Url}");
				args.Dispose();
				Persistent.Remove(key: args);
			}
		}

		public async Task ReConnectAsync()
		{
			while (true)
			{
				List<NodeInfo> reConnectSucess = new List<NodeInfo>();
				foreach (var reConnectQueue in reConnectQueues)
				{
					ILifetimeScope scope = lifetimeScope.BeginLifetimeScope();
					try
					{
						ClientWebSocket clientWebSocket = scope.Resolve<ClientWebSocket>();
						Uri uri = new Uri(reConnectQueue.Url);
						await clientWebSocket.ConnectAsync(uri, CancellationToken.None);
						RpcClient rpcClient = scope.Resolve<RpcClient>();
						await rpcClient.StartAsync(reConnectQueue);
						logger.LogInformation($"reConnect Success Id : {reConnectQueue.Name} Url : {reConnectQueue.Url}");
						Persistent.Add(scope, rpcClient);
						reConnectSucess.Add(rpcClient.NodeInfo);
						rpcClient.onDisconnected += Rpc_onDisconnected;
					}
					catch
					{
						logger.LogError($"reConnect Fail Id : {reConnectQueue.Name} Url : {reConnectQueue.Url}");
						IDisposer disposer = scope.Disposer;
						disposer.Dispose();
					}
				}
				if (reConnectSucess.Count > 0)
					foreach (var reConnectSuces in reConnectSucess)
					{
						reConnectQueues.Remove(reConnectSuces);
					}
				Thread.Sleep(10000);
			}
		}
		public RpcClient GetConnect()
		{
			RpcClient? rpcClient = null;
			long base_latency = long.MaxValue;
			foreach (var client in Persistent.Values)
			{
				long node_latency = client.Interval + client.Stopwatch.ElapsedMilliseconds;
				if (node_latency >= 0 && node_latency < base_latency)
				{
					base_latency = node_latency;
					rpcClient = client;
				}
			}
			if (rpcClient == null)
			{
				throw new Exception("Rpc 客户端池中未找到任何一个可用的Rpc客户端");
			}
			return rpcClient;
		}

		public IEnumerable<RpcClient> GetConnect(NodePool Tag)
		{
			var client_pool = Persistent.Values.Where(it => it.Delay <= 1200);
			List<RpcClient> rpcClients_frist = client_pool.Where(it => it.NodeInfo.Pools != null && it.NodeInfo.Pools.Contains(Tag)).OrderBy(it => it.Delay).ToList();
			List<RpcClient> rpcClients_second = client_pool.Where(it => it.NodeInfo.Pools == null || !it.NodeInfo.Pools.Contains(Tag)).OrderBy(it => it.Delay).ToList();
			rpcClients_frist.AddRange(rpcClients_second);
			if (rpcClients_frist == null)
			{
				throw new Exception("Rpc 客户端池中未找到任何一个可用的Rpc客户端");
			}
			return rpcClients_frist;
		}
	}
}
