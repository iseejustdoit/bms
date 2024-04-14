namespace bms.Leaf.Common
{
    public class AtomicLong
    {
        private long value;

        public AtomicLong(long initialValue)
        {
            value = initialValue;
        }

        public long Get()
        {
            return value;
        }

        public void Set(long newValue)
        {
            value = newValue;
        }

        public long GetAndIncrement()
        {
            return Interlocked.Increment(ref value);
        }
    }
}
