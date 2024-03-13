
using Microsoft.Extensions.Logging;

namespace Subspace.Agent.Core.Model.Method
{
    public class SubmitSolutionResponseMethod
    {
        private readonly ILogger logger;
        private readonly RpcClient rpcClient;
        public SubmitSolutionResponseMethod(ILogger<SubmitSolutionResponseMethod> logger, RpcClient rpcClient)
        {
            this.logger = logger;
            this.rpcClient = rpcClient;
        }
        public async Task InvokeAsync(SolutionResponse solutionResponse)
        {
            try
            {
                logger.LogTrace($"Method : submitSolutionResponse");
                Task task = await rpcClient.InvokeAsync<Task>("subspace_submitSolutionResponse", solutionResponse);
            }
            catch (Exception ex)
            {
                logger.LogError($"【异常类型】: {ex} \r\n 【异常信息】: {ex.Message} \r\n【错误源】 : {ex.Source}");
            }
        }
        public class SolutionDetails
        {
#nullable disable
            public string publicKey { get; set; }
            public string rewardAddress { get; set; }
            public UInt16 sectorIndex { get; set; }
            public UInt64 historySize { get; set; }
            public UInt16 pieceOffset { get; set; }
            public string recordCommitment { get; set; }
            public string recordWitness { get; set; }
            public string chunk { get; set; }
            public string chunkWitness { get; set; }
            public string proofOfSpace { get; set; }
#nullable disable
        }

        public class SolutionResponse
        {
            public UInt64 slotNumber { get; set; }
            public SolutionDetails solution { get; set; }
        }
    }
}
