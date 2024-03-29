﻿using Microsoft.Extensions.Logging;
using Polly;
using StreamJsonRpc;
using Subspace.Agent.Core.EventBus;
using Subspace.Agent.Core.Model;
using Subspace.Agent.Core.Model.Event;
using Subspace.Agent.Core.Model.Method;
using System.Collections.Concurrent;
using System.Diagnostics;
using static Subspace.Agent.Core.MessageConsumer;
using static Subspace.Agent.Core.Model.Event.RewardSigningEven;
using static Subspace.Agent.Core.Model.Event.SlotInfoEven;
using static Subspace.Agent.Core.Model.Method.FarmerInfoMethod;
using static Subspace.Agent.Core.Model.Method.SubmitSolutionResponseMethod;

namespace Subspace.Agent.Core
{
    public class RpcServer : IDisposable
    {
        public static AsyncLocal<string?> CurrentClientIp = new AsyncLocal<string?>();
        public event EventHandler<RpcServer>? Disconnected;
        private readonly Policy retryPolicy;

        private ILogger logger;
        private string? slotinfo_subscribeId;
        private string? rewardsigning_subscribeId;
        private string? segmentheader_subscribeId;
        private string? syncstatus_subscribeId;

        private JsonRpc Server;
        private readonly ClientPool clientPool;
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public RpcServer(ClientPool clientPool)
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        {
            this.clientPool = clientPool;
            retryPolicy = Policy
                .Handle<Exception>() // 指定应该在哪些异常发生时重试
                .Retry(2, (exception, retryCount) =>
                {
                    // 这里可以记录异常信息和重试次数
                    Console.WriteLine($"异常: {exception.Message}. 重试次数: {retryCount}");
                });
        }
        public void Start(WebSocketMessageHandler messageHandler, ILogger logger, string originalIpAddress)
        {
            // Server.TraceSource.Switch.Level = SourceLevels.All;
            // Server.TraceSource.Listeners.Add(new ConsoleTraceListener(logger));
            CurrentClientIp.Value = originalIpAddress;
            this.logger = logger;
            Server = new JsonRpc(messageHandler);
            Server.Disconnected += Server_Disconnected;
            Server.AddLocalRpcTarget(this);
            Server.StartListening();
        }
        private void Server_Disconnected(object? sender, JsonRpcDisconnectedEventArgs e)
        {
            Disconnected?.Invoke(this, this);
            Server.Disconnected -= Server_Disconnected;
            Dispose();
        }

        #region subscribe
        [JsonRpcMethod(name: "subspace_subscribeSlotInfo")]
        public string SubscribeSlotInfo()
        {
            slotinfo_subscribeId = Guid.NewGuid().ToString("N").Substring(0, 16);
            SlotInfoMessageClass.SubscribeSlotInfo.TryAdd(slotinfo_subscribeId, Server);
            return slotinfo_subscribeId;
        }
        [JsonRpcMethod(name: "subspace_unsubscribeSlotInfo")]
        public void UnsubscribeSlotInfo(string subscribeId)
        {
            SlotInfoMessageClass.SubscribeSlotInfo.TryRemove(subscribeId, out JsonRpc? jsonRpc);
        }
        [JsonRpcMethod(name: "subspace_subscribeRewardSigning")]
        public string SubscribeRewardSigning()
        {
            rewardsigning_subscribeId = Guid.NewGuid().ToString("N").Substring(0, 16);
            RewardSigningMessageClass.SubscribeRewardSigning.TryAdd(rewardsigning_subscribeId, Server);
            return rewardsigning_subscribeId;
        }
        [JsonRpcMethod(name: "subspace_unsubscribeRewardSigning")]
        public void UnsubscribeRewardSigning(string subscribeId)
        {
            RewardSigningMessageClass.SubscribeRewardSigning.TryRemove(subscribeId, out JsonRpc? jsonRpc);
        }
        [JsonRpcMethod(name: "subspace_subscribeArchivedSegmentHeader")]
        public string SubscribeArchivedSegmentHeader()
        {
            segmentheader_subscribeId = Guid.NewGuid().ToString("N").Substring(0, 16);
            SegmentHeaderClass.SubscribeSegmentHeader.TryAdd(segmentheader_subscribeId, Server);
            return segmentheader_subscribeId;
        }
        [JsonRpcMethod(name: "subspace_unsubscribeArchivedSegmentHeader")]
        public void UnsubscribeArchivedSegmentHeader(string subscribeId)
        {
            SegmentHeaderClass.SubscribeSegmentHeader.TryRemove(subscribeId, out JsonRpc? jsonRpc);
        }
        [JsonRpcMethod(name: "subspace_unsubscribeNodeSyncStatusChange")]
        public void UnsubscribeNodeSyncStatusChange(string subscribeId)
        {
            SyncStatusMessageClass.SubscribeSyncStatus.TryRemove(subscribeId, out JsonRpc? jsonRpc);
        }
        [JsonRpcMethod(name: "subspace_subscribeNodeSyncStatusChange")]
        public string SubscribeNodeSyncStatusChange()
        {
            syncstatus_subscribeId = Guid.NewGuid().ToString("N").Substring(0, 16);
            SyncStatusMessageClass.SubscribeSyncStatus.TryAdd(syncstatus_subscribeId, Server);
            new Task(() =>
            {
                Thread.Sleep(3000);
                _ = Server.NotifyWithParameterObjectAsync("subspace_node_sync_status_change", new NotifyMothod<NodeSyncStatus>(syncstatus_subscribeId, NodeSyncStatus.synced));
            }).Start();
            return syncstatus_subscribeId;
        }
        #endregion

        #region Invoke
        [JsonRpcMethod(name: "subspace_getFarmerAppInfo")]
        public async Task<FarmerAppInfo> GetFarmerAppInfoAsync()
        {
            RpcClient rpcClient = clientPool.GetConnect();
            return await rpcClient.getFarmerAppInfo.InvokeAsync();
        }
        [JsonRpcMethod(name: "subspace_submitRewardSignature")]
        public async Task SubmitRewardSignatureAsync(RewardSigningReques rewardSigningReques)
        {
            RpcClient rpcClient = clientPool.GetConnect();
            await rpcClient.submitRewardSignature.InvokeAsync(rewardSigningReques);
        }
        [JsonRpcMethod(name: "subspace_submitSolutionResponse")]
        public async Task SubmitSolutionResponseAsync(SolutionResponse solutionResponse)
        {
            RpcClient rpcClient = clientPool.GetConnect();
            await rpcClient.submitSolution.InvokeAsync(solutionResponse);
        }
        [JsonRpcMethod(name: "subspace_segmentHeaders")]
        public async Task<SegmentHeader[]?> SegmentHeadersAsync(UInt64[] segment_indexes)
        {
            SegmentHeader[]? segmentHeaders = null;
            await retryPolicy.Execute(async () =>
            {
                RpcClient rpcClient = clientPool.GetConnect();
                segmentHeaders = await rpcClient.segmentHeaders.InvokeAsync(segment_indexes);
            });
            return segmentHeaders;
        }
        [JsonRpcMethod(name: "subspace_piece")]
        public async Task<UInt16[]?> PieceAsync(UInt64 piece_index)
        {
            UInt16[]? piece = null;
            await retryPolicy.Execute(async () =>
            {
                RpcClient rpcClient = clientPool.GetConnect();
                piece = await rpcClient.pieceMethod.InvokeAsync(piece_index);
            });
            return piece;
        }
        [JsonRpcMethod(name: "subspace_acknowledgeArchivedSegmentHeader")]
        public async Task AcknowledgeArchivedSegmentHeaderAsync(UInt64 segment_index)
        {
            await retryPolicy.Execute(async () =>
            {
                RpcClient rpcClient = clientPool.GetConnect();
                await rpcClient.acknowledgeArchivedSegment.InvokeAsync(segment_index);
            });
        }
        [JsonRpcMethod(name: "subspace_lastSegmentHeaders")]
        public async Task<SegmentHeader[]?> LastSegmentHeadersAsync(UInt64 limit)
        {
            SegmentHeader[]? segmentHeaders = null;
            await retryPolicy.Execute(async () =>
            {
                RpcClient rpcClient = clientPool.GetConnect();
                segmentHeaders = await rpcClient.lastSegmentHeaders.InvokeAsync(limit);
            });
            return segmentHeaders;
        }
        #endregion

        public void Dispose()
        {
            if (slotinfo_subscribeId != null)
                UnsubscribeSlotInfo(slotinfo_subscribeId);
            if (rewardsigning_subscribeId != null)
                UnsubscribeRewardSigning(rewardsigning_subscribeId);
            if (segmentheader_subscribeId != null)
                UnsubscribeArchivedSegmentHeader(segmentheader_subscribeId);
            if (syncstatus_subscribeId != null)
                UnsubscribeNodeSyncStatusChange(syncstatus_subscribeId);
            Server.Dispose();
        }
    }
    public class MessageConsumer
    {
        public class SyncStatusMessageClass : IConsumer<NodeSyncStatus>
        {
            public static ConcurrentDictionary<string, JsonRpc> SubscribeSyncStatus = new ConcurrentDictionary<string, JsonRpc>();
            public async Task ConsumeAsync(RpcClient sender, NodeSyncStatus context)
            {
                var tasks = new List<Task>();
                foreach (KeyValuePair<string, JsonRpc> keyValuePair in SubscribeSyncStatus)
                {
                    if (!keyValuePair.Value.IsDisposed)
                    {
                        var task = keyValuePair.Value.NotifyWithParameterObjectAsync("subspace_node_sync_status_change", new NotifyMothod<NodeSyncStatus>(keyValuePair.Key, context));
                        tasks.Add(task);
                    }
                }
                await Task.WhenAll(tasks);
            }
        }
        public class SegmentHeaderClass : IConsumer<SegmentHeader>
        {
            public static ConcurrentDictionary<string, JsonRpc> SubscribeSegmentHeader = new ConcurrentDictionary<string, JsonRpc>();
            private UInt64 segment_index;
            public async Task ConsumeAsync(RpcClient sender, SegmentHeader context)
            {
                if (context.v0 != null && context.v0.segmentIndex > segment_index)
                {
                    var tasks = new List<Task>();
                    segment_index = context.v0.segmentIndex;
                    foreach (KeyValuePair<string, JsonRpc> keyValuePair in SubscribeSegmentHeader)
                    {
                        if (!keyValuePair.Value.IsDisposed)
                        {
                            var task = keyValuePair.Value.NotifyWithParameterObjectAsync("subspace_archived_segment_header", new NotifyMothod<SegmentHeader>(keyValuePair.Key, context));
                            tasks.Add(task);
                        }
                    }
                    await Task.WhenAll(tasks);
                }
            }
        }
        public class RewardSigningMessageClass : IConsumer<RewardSigningInfo>
        {
            public static ConcurrentDictionary<string, JsonRpc> SubscribeRewardSigning = new ConcurrentDictionary<string, JsonRpc>();
            private string? hash;
            public async Task ConsumeAsync(RpcClient sender, RewardSigningInfo context)
            {
                if (context.hash != null && !context.hash.Equals(hash))
                {
                    hash = context.hash;
                    var tasks = new List<Task>();
                    foreach (KeyValuePair<string, JsonRpc> keyValuePair in SubscribeRewardSigning)
                    {
                        if (!keyValuePair.Value.IsDisposed)
                        {
                            var task = keyValuePair.Value.NotifyWithParameterObjectAsync("subspace_reward_signing", new NotifyMothod<RewardSigningInfo>(keyValuePair.Key, context));
                            tasks.Add(task);
                        }
                    }
                    await Task.WhenAll(tasks);
                }
            }
        }
        public class SlotInfoMessageClass : IConsumer<SlotInfo>
        {
            public static ConcurrentDictionary<string, JsonRpc> SubscribeSlotInfo = new ConcurrentDictionary<string, JsonRpc>();
            private ulong last_slotNumber;
            private readonly Stopwatch stopwatch = new Stopwatch();
            public async Task ConsumeAsync(RpcClient sender, SlotInfo context)
            {
                if (context.slotNumber > last_slotNumber)
                {
                    sender.Latency = 0;
                    sender.logger.LogDebug($"Imported #{context.slotNumber} from {sender.NodeInfo.name} interval {stopwatch.ElapsedMilliseconds}");
                    stopwatch.Restart();
                    stopwatch.Start();
                    last_slotNumber = context.slotNumber;
                    var tasks = new List<Task>();
                    foreach (KeyValuePair<string, JsonRpc> keyValuePair in SubscribeSlotInfo)
                    {
                        if (!keyValuePair.Value.IsDisposed)
                        {
                            var task = keyValuePair.Value.NotifyWithParameterObjectAsync("subspace_slot_info", new NotifyMothod<SlotInfo>(keyValuePair.Key, context));
                            tasks.Add(task);
                        }
                    }
                    await Task.WhenAll(tasks);
                }
                else
                {
                    // 这里设置延迟为0+x
                    sender.Latency = stopwatch.Elapsed.TotalMilliseconds;
                    // sender.logger.LogDebug($"Imported #{context.slotNumber} from {sender.NodeInfo.name} interval {stopwatch.ElapsedMilliseconds}");
                }

            }
        }
        public class NotifyMothod<T>
        {
            public NotifyMothod(string subscription, T result)
            {
                this.subscription = subscription;
                this.result = result;
            }
            public string subscription { get; set; }
            public T result { get; set; }
        }
    }
}