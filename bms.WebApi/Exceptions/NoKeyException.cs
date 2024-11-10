namespace bms.WebApi.Exceptions
{
    /// <summary>
    /// 表示在找不到键时引发的异常。
    /// </summary>
    public class NoKeyException : Exception
    {
        /// <summary>
        /// 使用默认错误消息初始化 <see cref="NoKeyException"/> 类的新实例。
        /// </summary>
        public NoKeyException()
            : base("Key is none")
        {
        }
    }
}
