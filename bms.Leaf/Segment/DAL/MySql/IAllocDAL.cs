using bms.Leaf.Segment.Model;

namespace bms.Leaf.Segment.DAL.MySql
{
    public interface IAllocDAL
    {
        List<LeafAllocModel> GetAllLeafAllocs();
        Task<LeafAllocModel> UpdateMaxIdAndGetLeafAllocAsync(string tag);
        Task<LeafAllocModel> UpdateMaxIdByCustomStepAndGetLeafAllocAsync(LeafAllocModel leafAlloc);
        Task<List<string>> GetAllTagsAsync();
    }
}
