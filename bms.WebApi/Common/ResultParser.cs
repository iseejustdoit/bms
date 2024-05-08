using bms.Leaf.Common;
using bms.WebApi.Exceptions;

namespace bms.WebApi.Common
{
    public class ResultParser
    {
        public static string ParseResult(string key, Result result)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new NoKeyException();
            }
            if (result.Status == Status.EXCEPTION)
            {
                throw new LeafServerException(result.ToString());
            }
            return result.Id.ToString();
        }
    }
}
