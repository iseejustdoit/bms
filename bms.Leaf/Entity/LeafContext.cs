using FreeSql;

namespace bms.Leaf.Entity
{
    public class LeafContext : DbContext
    {
        public required DbSet<LeafAlloc> LeafAlloc { get; set; }
    }
}
