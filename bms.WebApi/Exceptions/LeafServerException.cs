namespace bms.WebApi.Exceptions
{
    /// <summary>
    /// 表示在Leaf服务器执行应用程序期间发生的错误。
    /// </summary>
    /// <remarks>
    /// 使用指定的错误消息初始化 <see cref="LeafServerException"/> 类的新实例。
    /// </remarks>
    /// <param name="message">描述错误的消息。</param>
    public class LeafServerException(string message) : Exception(message)
    {
    }
}
