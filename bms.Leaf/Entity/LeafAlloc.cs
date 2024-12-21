using FreeSql.DataAnnotations;

namespace bms.Leaf.Entity
{
    [Table(Name = "alloc")]
    public class LeafAlloc
    {
        [Column(Name = "biz_tag", IsPrimary = true)]
        public required string BizTag { get; set; }
        [Column(Name = "max_id")]
        public long MaxId { get; set; }
        [Column(Name = "step")]
        public int Step { get; set; }
        [Column(Name = "description")]
        public string? Description { get; set; }
        [Column(Name = "update_time")]
        public DateTime UpdateTime { get; set; }
    }
}
