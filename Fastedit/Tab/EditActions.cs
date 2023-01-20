using Fastedit.Settings;

namespace Fastedit.Tab
{
    public class EditActions
    {
        public static void Copy(TabPageItem tab)
        {
            if (tab == null)
                return;

            tab.textbox.Copy();
        }
        public static void Paste(TabPageItem tab)
        {
            if (tab == null)
                return;

            tab.textbox.Paste();
        }
        public static void Cut(TabPageItem tab)
        {
            if (tab == null)
                return;

            tab.textbox.Cut();
        }
        public static void SelectAll(TabPageItem tab)
        {
            if (tab == null)
                return;
            tab.textbox.SelectAll();
        }
        public static void Undo(TabPageItem tab)
        {
            if (tab == null)
                return;

            tab.textbox.Undo();
        }
        public static void Redo(TabPageItem tab)
        {
            if (tab == null)
                return;

            tab.textbox.Redo();
        }
        public static void DuplicateLine(TabPageItem tab)
        {
            if (tab == null)
                return;

            tab.textbox.DuplicateLine(tab.textbox.CursorPosition.LineNumber);
        }
        public static void ZoomIn(TabPageItem tab)
        {
            if (tab == null)
                return;
            tab.textbox.ZoomFactor += DefaultValues.ZoomSteps;
        }
        public static void ZoomOut(TabPageItem tab)
        {
            if (tab == null)
                return;

            tab.textbox.ZoomFactor -= DefaultValues.ZoomSteps;
        }

        public static void GoToLine(TabPageItem tab, int line)
        {
            if (tab == null)
                return;

            tab.textbox.GoToLine(line);
            tab.textbox.ScrollLineIntoView(line);
            tab.textbox.Focus(Windows.UI.Xaml.FocusState.Programmatic);
        }
    }
}
