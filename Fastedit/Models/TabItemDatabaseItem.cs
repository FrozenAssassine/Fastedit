namespace Fastedit.Models
{
    public class TabItemDatabaseItem
    {
        public bool IsModified { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string Identifier { get; set; }
        public int SelectedIndex { get; set; }
        public string FileToken { get; set; }
        public int ZoomFactor { get; set; }
        public string CodeLanguage { get; set; }
        public int Encoding { get; set; }
        public bool HasOwnWindow { get; set; }
    }
}
