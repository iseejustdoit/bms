namespace bms.Leaf.Common
{
    public class AtomicBoolean
    {
        private int value;
        public AtomicBoolean(bool initialValue)
        {
            value = initialValue ? 1 : 0;
        }

        public bool Get()
        {
            return value == 1;
        }

        public void Set(bool newValue)
        {
            value = newValue ? 1 : 0;
        }

        public bool CompareAndSet(bool expect, bool update)
        {
            int expectedValue = expect ? 1 : 0;
            int newValue = update ? 1 : 0;

            return Interlocked.CompareExchange(ref value, newValue, expectedValue) == expectedValue;
        }
    }
}
