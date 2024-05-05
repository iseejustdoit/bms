using bms.Leaf.Segment.Model;
using Microsoft.EntityFrameworkCore;

namespace bms.Leaf.Segment.Entity
{
    public class LeafContext : DbContext
    {
        public LeafContext(DbContextOptions<LeafContext> options) : base(options)
        {

        }

        public DbSet<LeafAlloc> LeafAlloc { get; set; }
    }
}
