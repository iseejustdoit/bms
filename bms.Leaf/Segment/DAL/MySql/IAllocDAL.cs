using bms.Leaf.Segment.Model;

namespace bms.Leaf.Segment.DAL.MySql
{
    public interface IAllocDAL
    {
        Task<LeafAllocModel> UpdateMaxIdAndGetLeafAllocAsync(string tag, CancellationToken cancellationToken = default);
        Task<LeafAllocModel> UpdateMaxIdByCustomStepAndGetLeafAllocAsync(LeafAllocModel leafAlloc, CancellationToken cancellationToken = default);
        Task<List<string>> GetAllTagsAsync(CancellationToken cancellationToken = default);
    }
}
