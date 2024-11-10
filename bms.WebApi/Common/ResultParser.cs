using bms.Leaf.Common;
using bms.WebApi.Exceptions;

namespace bms.WebApi.Common
{
    /// <summary>
    /// 提供解析结果的方法。
    /// </summary>
    public class ResultParser
    {
        /// <summary>
        /// 解析结果并返回结果ID作为字符串。
        /// </summary>
        /// <param name="key">与结果关联的键。</param>
        /// <param name="result">要解析的结果。</param>
        /// <returns>结果ID作为字符串。</returns>
        /// <exception cref="NoKeyException">当键为空或null时抛出。</exception>
        /// <exception cref="LeafServerException">当结果状态为异常时抛出。</exception>
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
