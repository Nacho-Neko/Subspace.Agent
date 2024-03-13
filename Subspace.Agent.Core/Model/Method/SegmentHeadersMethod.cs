using Microsoft.Extensions.Logging;

namespace Subspace.Agent.Core.Model.Method
{
    public class SegmentHeadersMethod
    {
        private readonly ILogger logger;
        private readonly RpcClient rpcClient;
        public SegmentHeadersMethod(ILogger<SegmentHeadersMethod> logger, RpcClient rpcClient)
        {
            this.logger = logger;
            this.rpcClient = rpcClient;
        }
        public async Task<SegmentHeader[]?> InvokeAsync(UInt64[] segment_index)
        {
            try
            {
                logger.LogTrace($"Method : subspace_segmentHeaders");
                SegmentHeader[]? segmentHeader = await rpcClient.InvokeAsync<SegmentHeader[]?>("subspace_segmentHeaders", segment_index);
                logger.LogTrace($"Result : {segmentHeader}");
                return segmentHeader;
            }
            catch (Exception ex)
            {
                logger.LogError($"【异常类型】: {ex.ToString()} \r\n 【异常信息】: {ex.Message} \r\n【错误源】 : {ex.Source}");
                return null;
            }
        }
    }
}