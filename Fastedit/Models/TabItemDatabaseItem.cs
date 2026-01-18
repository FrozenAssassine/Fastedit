using System.Text.Json.Serialization;
using TextControlBoxNS;

namespace Fastedit.Models
{
    public class TabItemDatabaseItem
    {
        public bool IsModified { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string Identifier { get; set; }
        public int SelectedIndex { get; set; }
        public int ZoomFactor { get; set; }
        public string CodeLanguage { get; set; }
        public int Encoding { get; set; }
        public bool HasOwnWindow { get; set; }
        public int CharacterPos { get; set; }
        public int LinePos { get; set; }
        public LineEnding LineEnding { get; set; }
        public bool? WhitespaceCharacters { get; set; } = null;
        public int TabsSpaces { get; set; } = -1;
        public bool IsReadOnly { get; set; } = false;
        [JsonIgnore]
        public bool WasNeverSaved => this.FilePath.Length == 0;
    }
}
