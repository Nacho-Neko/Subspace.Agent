using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Subspace.Agent
{
    internal class ConsoleTraceListener : TraceListener
    {
        private ILogger logger;
        public ConsoleTraceListener(ILogger logger)
        {
            this.logger = logger;
        }
        public override void Write(string? message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                if (message.Contains("subspace_slot_info"))
                {
                    return;
                }
				if (message.Contains("Invoking"))
				{
					return;
				}
				if (message.Contains("JsonRpc"))
				{
					return;
				}
				logger.LogInformation(message);
            }
        }

        public override void WriteLine(string? message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                if (message.Contains("subspace_slot_info"))
                {
                    return;
                }
				if (message.Contains("Invoking"))
				{
					return;
				}
				if (message.Contains("JsonRpc"))
				{
					return;
				}
				logger.LogInformation(message);
            }
        }
    }
}