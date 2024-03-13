using Microsoft.Extensions.Logging;

namespace Subspace.Agent.Core.Model.Method
{
    public class LastSegmentHeadersMethod
    {
        private readonly ILogger logger;
        private readonly RpcClient rpcClient;
        public LastSegmentHeadersMethod(ILogger<LastSegmentHeadersMethod> logger, RpcClient rpcClient)
        {
            this.logger = logger;
            this.rpcClient = rpcClient;
        }
        public async Task<SegmentHeader[]?> InvokeAsync(UInt64 limit)
        {
            try
            {
                logger.LogTrace($"Method : subspace_lastSegmentHeaders");
                SegmentHeader[]? segmentHeaders = await rpcClient.InvokeAsync<SegmentHeader[]?>("subspace_lastSegmentHeaders", limit);
                logger.LogTrace($"Result : {segmentHeaders}");
                return segmentHeaders;
            }
            catch (Exception ex)
            {
                logger.LogError($"【异常类型】: {ex.ToString()} \r\n 【异常信息】: {ex.Message} \r\n【错误源】 : {ex.Source}");
                return null;
            }
        }
    }
}