namespace WpfLeafTest
{
    public interface ITextService
    {
        public string GetText();
        void Plus();
    }

    public class TextService : ITextService
    {
        int count = 0;
        public TextService()
        {

        }
        public string GetText()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }

        public void Plus()
        {
            count++;
        }
    }
}
