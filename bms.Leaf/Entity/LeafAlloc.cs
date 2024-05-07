using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bms.Leaf.Entity
{
    [Table("Alloc")]
    public class LeafAlloc
    {
        [Key]
        public string BizTag { get; set; }
        public long MaxId { get; set; }
        public int Step { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}
