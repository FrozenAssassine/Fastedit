using Windows.ApplicationModel.DataTransfer;

namespace Fastedit.Helper
{
    internal class ClipboardHelper
    {
        public static void Copy(string text)
        {
            var package = new DataPackage();
            package.SetText(text);
            Clipboard.SetContent(package);
        }
    }
}
