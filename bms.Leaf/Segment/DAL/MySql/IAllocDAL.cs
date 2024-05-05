using bms.Leaf.Segment.Model;

namespace bms.Leaf.Segment.DAL.MySql
{
    public interface IAllocDAL
    {
        Task<LeafAlloc> UpdateMaxIdAndGetLeafAllocAsync(string tag, CancellationToken cancellationToken = default);
        Task<LeafAlloc> UpdateMaxIdByCustomStepAndGetLeafAllocAsync(LeafAlloc leafAlloc, CancellationToken cancellationToken = default);

        LeafAlloc UpdateMaxIdAndGetLeafAlloc(string tag);
        LeafAlloc UpdateMaxIdByCustomStepAndGetLeafAlloc(LeafAlloc leafAlloc);
        Task<List<string>> GetAllTagsAsync(CancellationToken cancellationToken = default);
    }
}
