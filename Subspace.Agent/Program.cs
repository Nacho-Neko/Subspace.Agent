using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using StreamJsonRpc;
using Subspace.Agent.Core;
using Subspace.Agent.Core.EventBus;
using Subspace.Agent.Core.Model;
using Subspace.Agent.Core.Model.Event;
using Subspace.Agent.Core.Model.Method;
using Subspace.Agent.EventBus;
using System.Net.WebSockets;
using YamlDotNet.Serialization;

var configuration = new ConfigurationBuilder()
    .AddYamlFile("appsettings.yaml", optional: true, reloadOnChange: true) // 添加配置文件
    .Build();

Log.Logger = new LoggerConfiguration()
   .Enrich.With(new ShortSourceContextEnricher())
   .Enrich.FromLogContext()
   .ReadFrom.Configuration(configuration)
   .CreateLogger();

var loggerFactory = new LoggerFactory().AddSerilog(Log.Logger);

var builder = new ContainerBuilder();
builder.RegisterInstance(loggerFactory).As<ILoggerFactory>();
builder.RegisterGeneric(typeof(Logger<>)).As(typeof(ILogger<>)).SingleInstance();
builder.RegisterType<WebSocketService>().SingleInstance();
builder.RegisterType<ClientPool>().SingleInstance();

builder.RegisterType<ClientWebSocket>().InstancePerLifetimeScope();
builder.Register(o =>
{
    ClientWebSocket clientWebSocket = o.Resolve<ClientWebSocket>();
    WebSocketMessageHandler webSocketMessageHandler = new WebSocketMessageHandler(clientWebSocket);
    return webSocketMessageHandler;
}).InstancePerLifetimeScope();


builder.RegisterType<EventBus>().As<IBusControl>().SingleInstance();
builder.RegisterType<RpcClient>().InstancePerLifetimeScope();

builder.RegisterType<ArchivedSegmentHeaderEven>().InstancePerLifetimeScope();
builder.RegisterType<NodeSyncStatusChangeEven>().InstancePerLifetimeScope();
builder.RegisterType<RewardSigningEven>().InstancePerLifetimeScope();
builder.RegisterType<SlotInfoEven>().InstancePerLifetimeScope();

builder.RegisterType<AcknowledgeArchivedSegmentHeaderMethod>().InstancePerLifetimeScope();
builder.RegisterType<FarmerInfoMethod>().InstancePerLifetimeScope();
builder.RegisterType<LastSegmentHeadersMethod>().InstancePerLifetimeScope();
builder.RegisterType<PieceMethod>().InstancePerLifetimeScope();
builder.RegisterType<SegmentHeadersMethod>().InstancePerLifetimeScope();
builder.RegisterType<SubmitRewardSignatureMethod>().InstancePerLifetimeScope();
builder.RegisterType<SubmitSolutionResponseMethod>().InstancePerLifetimeScope();

var container = builder.Build();
var busControl = container.Resolve<IBusControl>();
busControl.Start();

var yamlDeserializer = new DeserializerBuilder().Build();
var yaml = File.ReadAllText("config.yaml");
var config = yamlDeserializer.Deserialize<ConfigModel>(yaml);

ClientPool clientPool = container.Resolve<ClientPool>();
await clientPool.StartAsync(config);
WebSocketService webSocket = container.Resolve<WebSocketService>();
await webSocket.StartAsync(config);