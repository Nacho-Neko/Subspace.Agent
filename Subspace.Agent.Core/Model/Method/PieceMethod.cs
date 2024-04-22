using Microsoft.Extensions.Logging;
using System;

namespace Subspace.Agent.Core.Model.Method
{
    public class PieceMethod
    {
        private readonly ILogger logger;
        private readonly RpcClient rpcClient;
        public PieceMethod(ILogger<PieceMethod> logger, RpcClient rpcClient)
        {
            this.logger = logger;
            this.rpcClient = rpcClient;
        }
        public async Task<ArraySegment<UInt16>?> InvokeAsync(UInt64 piece_index)
        {
            try
            {
                logger.LogTrace($"Method : subspace_piece");
                ArraySegment<UInt16>? resutl = await rpcClient.InvokeAsync<ArraySegment<UInt16>?>("subspace_piece", piece_index);
                logger.LogTrace($"Result : {resutl}");
                return resutl;
            }
            catch (Exception ex)
            {
                logger.LogError($"【异常类型】: {ex.ToString()} \r\n 【异常信息】: {ex.Message} \r\n【错误源】 : {ex.Source}");
                return null;
            }
        }
    }
}
