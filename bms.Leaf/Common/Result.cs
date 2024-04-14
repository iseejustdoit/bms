using System.Text;

namespace bms.Leaf.Common
{
    public class Result
    {
        public long Id { get; set; }
        public Status Status { get; set; }

        public Result()
        {

        }

        public Result(long id, Status status)
        {
            this.Id = id;
            this.Status = status;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("Result{");
            sb.Append("id=").Append(Id);
            sb.Append(", status=").Append(Status);
            sb.Append('}');
            return sb.ToString();
        }
    }
}
