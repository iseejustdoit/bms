namespace bms.WebApi.Exceptions
{
    public class NoKeyException : Exception
    {
        public NoKeyException()
            : base("Key is none")
        {
        }
    }
}
