namespace Subspace.Agent.Core.Model
{
    public class SegmentHeader
    {
        public V0? v0 { get; set; }
        public class V0
        {
            public UInt64 segmentIndex { get; set; }
            public string? segmentCommitment { get; set; }
#nullable disable
            public UInt16[] prevSegmentHeaderHash { get; set; }
            public LastArchivedBlock lastArchivedBlock { get; set; }
#nullable enable
        }
        public class LastArchivedBlock
        {
#nullable disable
            public UInt32 number { get; set; }
            public object archivedProgress { get; set; }
#nullable enable

        }
    }
    public enum ArchivedBlockProgress
    {
        Complete,
        Partial,
    }
}
