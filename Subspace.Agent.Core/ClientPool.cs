using Autofac;
using Autofac.Core;
using Microsoft.Extensions.Logging;
using Subspace.Agent.Core.Model;
using System.Net.WebSockets;
using System.Runtime.Serialization;

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
            List<NodeInfo> nodes = configModel.node;
            foreach (var node in nodes)
            {
                var scope = lifetimeScope.BeginLifetimeScope(node.name);
                NodeInfo nodeInfo = node;
                try
                {
                    ClientWebSocket clientWebSocket = scope.Resolve<ClientWebSocket>();
                    Uri uri = new Uri(node.url);
                    await clientWebSocket.ConnectAsync(uri, CancellationToken.None);
                    RpcClient rpcClient = scope.Resolve<RpcClient>();
                    await rpcClient.StartAsync(node);
                    Persistent.Add(scope, rpcClient);
                    logger.LogInformation($"Connect RpcClient {Persistent.Count} Url : {nodeInfo.url}");
                }
                catch
                {
                    reConnectQueues.Add(nodeInfo);
                    scope.Dispose();
                    logger.LogError($"Connect Fail RpcClient {Persistent.Count} Url : {nodeInfo.url}");
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
                logger.LogInformation($"Disconnect RpcClient {Persistent.Count} Id : {socketRpc.NodeInfo.name} Url : {socketRpc.NodeInfo.url}");
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
                        Uri uri = new Uri(reConnectQueue.url);
                        await clientWebSocket.ConnectAsync(uri, CancellationToken.None);
                        RpcClient rpcClient = scope.Resolve<RpcClient>();
                        await rpcClient.StartAsync(reConnectQueue);
                        logger.LogInformation($"reConnect Success Id : {reConnectQueue.name} Url : {reConnectQueue.url}");
                        Persistent.Add(scope, rpcClient);
                        reConnectSucess.Add(rpcClient.NodeInfo);
                        rpcClient.onDisconnected += Rpc_onDisconnected;
                    }
                    catch
                    {
                        logger.LogError($"reConnect Fail Id : {reConnectQueue.name} Url : {reConnectQueue.url}");
                        IDisposer disposer = scope.Disposer;
                        disposer.Dispose();
                    }
                }
                if (reConnectSucess.Count > 0)
                    foreach (var reConnectSuces in reConnectSucess)
                    {
                        reConnectQueues.Remove(reConnectSuces);
                    }
                Thread.Sleep(3000);
            }
        }
        public RpcClient GetConnect()
        {
            RpcClient? rpcClient = null;
            double Lowest = double.MaxValue;
            foreach (var Value in Persistent.Values)
            {
                double Latency = Value.Latency;
                if (Latency >= 0 && Latency < Lowest)
                {
                    Lowest = Latency;
                    rpcClient = Value;
                }
            }
            if (rpcClient == null)
            {
                throw new Exception("Rpc 客户端池中未找到任何一个可用的Rpc客户端");
            }
            return rpcClient;
        }
    }
}
