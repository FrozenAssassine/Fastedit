namespace Fastedit.Models
{
    public class OldDBItem
    {
        public string TabName { get; set; }
        public bool TabModified { get; set; }
        public string TabHeader { get; set; }
        public string TabToken { get; set; }
        public string TabPath { get; set; }
        public double ZoomFactor { get; set; }
        public int CurrentSelectedTabIndex { get; set; }
    }
}
