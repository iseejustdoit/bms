namespace bms.Leaf.Common
{
    public static class TaskExtension
    {
        public static async void Await(this Task task, Action? onCompleted = null, Action<Exception>? onError = null)
        {
            try
            {
                await task;
                onCompleted?.Invoke();
            }
            catch (Exception e)
            {
                onError?.Invoke(e);
            }
        }
    }
}
