using Autofac;
using Microsoft.Extensions.Logging;
using StreamJsonRpc;
using Subspace.Agent.Core.Model;
using System.Net;
using System.Net.WebSockets;

namespace Subspace.Agent.Core
{
	public class WebSocketService
	{
		private int ConnectCount;
		private readonly ILogger logger;
		private readonly ILifetimeScope lifetimeScope;
		private readonly ClientPool clientPool;
		private HttpListener listener;
		public WebSocketService(ILifetimeScope lifetimeScope, ILogger<WebSocketService> logger, ClientPool clientPool)
		{
			listener = new HttpListener();
			this.logger = logger;
			this.lifetimeScope = lifetimeScope;
			this.clientPool = clientPool;
		}
		public async Task StartAsync(ConfigModel configModel)
		{
			listener.Prefixes.Add(configModel.Listen);
			logger.LogInformation($"Start Listener : {configModel.Listen}");
			listener.Start();
			await ListenerAsync();
		}
		public async Task ListenerAsync()
		{
			while (true)
			{
				var context = await listener.GetContextAsync();
				if (context.Request.IsWebSocketRequest)
				{

					_ = HandleWebSocketAsync(context);
				}
				else
				{
					context.Response.StatusCode = 401;
					context.Response.Close();
				}
			}
		}
		async Task HandleWebSocketAsync(HttpListenerContext context)
		{
			string originalIpAddress;
			string? X_Forwarded_For = context.Request.Headers["X-Forwarded-For"];
			if (!string.IsNullOrEmpty(X_Forwarded_For))
				originalIpAddress = X_Forwarded_For;
			else
				originalIpAddress = context.Request.RemoteEndPoint.Address.ToString(); ;

			Interlocked.Increment(ref ConnectCount);
			logger.LogInformation($"connection remote_addr ={originalIpAddress} Accepting new connection {ConnectCount}/100");
			HttpListenerWebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null);
			WebSocket serverWebSocket = webSocketContext.WebSocket;
			WebSocketMessageHandler webSocketMessageHandler = new WebSocketMessageHandler(serverWebSocket);
			RpcServer rpcServer = new RpcServer(clientPool);
			rpcServer.Start(webSocketMessageHandler, logger, originalIpAddress);
			rpcServer.Disconnected += RpcServer_Disconnected;
		}

		private void RpcServer_Disconnected(object? sender, RpcServer e)
		{
			string ipaddr;
			if (sender != null)
			{
				AsyncLocal<string?>? CurrentClientIp = (AsyncLocal<string?>)sender;
				ipaddr = CurrentClientIp.Value ?? "null";
			}
			else
				ipaddr = "null";
			e.Disconnected -= RpcServer_Disconnected;
			Interlocked.Decrement(ref ConnectCount);
			logger.LogInformation($"Disconnected remote_addr ={ipaddr} connection {ConnectCount}/100");
		}
	}
}
