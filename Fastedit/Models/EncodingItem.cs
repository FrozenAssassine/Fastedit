using System.Text;

namespace Fastedit.Models
{
    public struct EncodingItem
    {
        public EncodingItem(Encoding encoding, string name)
        {
            this.encoding = encoding;
            this.name = name;
        }
        public Encoding encoding;
        public string name;
    }
}
