using bms.Leaf.Common;
using System.Text;

namespace bms.Leaf.Segment.Model
{
    public class SegmentModel
    {
        private AtomicLong value = new AtomicLong(0);
        private long max;
        private int step;
        private SegmentBufferModel buffer;

        public SegmentModel(SegmentBufferModel buffer)
        {
            this.buffer = buffer;
        }

        public AtomicLong Value
        {
            get { return value; }
            set { this.value = value; }
        }

        public long Max
        {
            get { return max; }
            set { max = value; }
        }

        public int Step
        {
            get { return step; }
            set { step = value; }
        }

        public SegmentBufferModel Buffer
        {
            get { return buffer; }
        }

        public long GetIdle()
        {
            return this.Max - Value.Get();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("Segment(");
            sb.Append("value:");
            sb.Append(value);
            sb.Append(",max:");
            sb.Append(max);
            sb.Append(",step:");
            sb.Append(step);
            sb.Append(")");
            return sb.ToString();
        }
    }
}
