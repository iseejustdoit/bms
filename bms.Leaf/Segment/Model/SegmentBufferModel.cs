using bms.Leaf.Common;
using System.Text;

namespace bms.Leaf.Segment.Model
{
    public class SegmentBufferModel
    {
        private string key;
        private SegmentModel[] segments; // Double buffer
        private int currentPos; // Current index of the SegmentModel in use
        private bool nextReady; // Whether the next SegmentModel is ready to switch
        private bool initOk; // Whether initialization is complete
        private readonly AtomicBoolean threadRunning; // Whether the thread is running

        private int step;
        private int minStep;
        private long updateTimestamp;
        public SegmentBufferModel()
        {
            segments = [new SegmentModel(this), new SegmentModel(this)];
            currentPos = 0;
            nextReady = false;
            initOk = false;
            threadRunning = new AtomicBoolean(false);
        }

        public string Key
        {
            get { return key; }
            set { key = value; }
        }
        public SegmentModel[] Segments
        {
            get { return segments; }
        }
        public SegmentModel Current
        {
            get { return segments[currentPos]; }
        }

        public int CurrentPos
        {
            get { return currentPos; }
        }

        public int NextPos()
        {
            return (currentPos + 1) % 2;
        }

        public void SwitchPos()
        {
            currentPos = NextPos();
        }

        public bool IsInitOk
        {
            get { return initOk; }
            set { initOk = value; }
        }

        public bool IsNextReady
        {
            get { return nextReady; }
            set { nextReady = value; }
        }
        public AtomicBoolean ThreadRunning
        {
            get { return threadRunning; }
        }

        public int Step
        {
            get { return step; }
            set { step = value; }
        }

        public int MinStep
        {
            get { return minStep; }
            set { minStep = value; }
        }

        public long UpdateTimestamp
        {
            get { return updateTimestamp; }
            set { updateTimestamp = value; }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("SegmentBuffer{");
            sb.Append("key='").Append(key).Append('\'');
            sb.Append(", segments=").Append(string.Join(", ", segments.Select(s => s.ToString())));
            sb.Append(", currentPos=").Append(currentPos);
            sb.Append(", nextReady=").Append(nextReady);
            sb.Append(", initOk=").Append(initOk);
            sb.Append(", threadRunning=").Append(threadRunning);
            sb.Append(", step=").Append(step);
            sb.Append(", minStep=").Append(minStep);
            sb.Append(", updateTimestamp=").Append(updateTimestamp);
            sb.Append('}');
            return sb.ToString();
        }
    }

}
