using Microsoft.Extensions.Logging;

namespace Subspace.Agent.Core.Model.Method
{
    public class AcknowledgeArchivedSegmentHeaderMethod
    {
        private readonly ILogger logger;
        private readonly RpcClient rpcClient;
        public AcknowledgeArchivedSegmentHeaderMethod(ILogger<AcknowledgeArchivedSegmentHeaderMethod> logger, RpcClient rpcClient)
        {
            this.logger = logger;
            this.rpcClient = rpcClient;
        }
        public async Task InvokeAsync(UInt64 segment_index)
        {
            try
            {
                logger.LogTrace($"Method : subspace_acknowledgeArchivedSegmentHeader");
                Task resutl = await rpcClient.InvokeAsync<Task>("subspace_acknowledgeArchivedSegmentHeader", segment_index);
            }
            catch (Exception ex)
            {
                logger.LogError($"【异常类型】: {ex.ToString()} \r\n 【异常信息】: {ex.Message} \r\n【错误源】 : {ex.Source}");
            }
        }
    }
}