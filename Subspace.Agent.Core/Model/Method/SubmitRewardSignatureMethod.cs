using Microsoft.Extensions.Logging;

namespace Subspace.Agent.Core.Model.Method
{
    public class SubmitRewardSignatureMethod
    {
        private readonly ILogger logger;
        private readonly RpcClient rpcClient;
        public SubmitRewardSignatureMethod(ILogger<SubmitRewardSignatureMethod> logger, RpcClient rpcClient)
        {
            this.logger = logger;
            this.rpcClient = rpcClient;
        }
        public async Task InvokeAsync(RewardSigningReques rewardSigningReques)
        {
            try
            {
                logger.LogTrace($"Method : subspace_submitRewardSignature");
                object task = await rpcClient.InvokeAsync<object>("subspace_submitRewardSignature", rewardSigningReques);
            }
            catch (Exception ex)
            {
                logger.LogError($"【异常类型】: {ex.ToString()} \r\n 【异常信息】: {ex.Message} \r\n【错误源】 : {ex.Source}");
            }
        }
    }
    public class RewardSigningReques
    {
        public string? hash { get; set; }
        public UInt16[]? signature { get; set; }
    }
}