using Microsoft.Extensions.Logging;

namespace Subspace.Agent.Core.Model.Method
{
    public class FarmerInfoMethod
    {
        private readonly ILogger logger;
        private readonly RpcClient rpcClient;
        public FarmerInfoMethod(ILogger<FarmerInfoMethod> logger, RpcClient rpcClient)
        {
            this.logger = logger;
            this.rpcClient = rpcClient;
        }
        public async Task<FarmerAppInfo> InvokeAsync()
        {
            logger.LogTrace($"Method : getFarmerAppInfo");
            try
            {
                FarmerAppInfo farmerApp = await rpcClient.InvokeAsync<FarmerAppInfo>("subspace_getFarmerAppInfo");
                if (farmerApp == null)
                {
                    throw new Exception($"{rpcClient.NodeInfo.name} farmerAppInfo 返回是空的");
                }
                logger.LogTrace($"Result : {farmerApp}");
                return farmerApp;
            }
            catch (Exception ex)
            {
                logger.LogError($"【异常类型】: {ex.ToString()} \r\n 【异常信息】: {ex.Message} \r\n【错误源】 : {ex.Source}");
                throw;
            }
        }

        public class FarmerAppInfo
        {
#nullable disable
            public string genesisHash { get; set; }
            public List<string> dsnBootstrapNodes { get; set; }
            public bool syncing { get; set; }
            public FarmingTimeout farmingTimeout { get; set; }
            public ProtocolInfo protocolInfo { get; set; }
            public class ProtocolInfo
            {
                public UInt64 historySize { get; set; }
                public UInt16 maxPiecesInSector { get; set; }
                public UInt64 recentSegments { get; set; }
                public UInt64[] recentHistoryFraction { get; set; }
                public UInt64 minSectorLifetime { get; set; }
            }
            public class FarmingTimeout
            {
                public UInt64 secs { get; set; }
                public UInt32 nanos { get; set; }
            }
#nullable enable
        }
    }
}
