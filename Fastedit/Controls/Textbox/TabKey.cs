using Fastedit.Core;
using System;

namespace Fastedit.Controls.Textbox
{
    public class TabKey
    {
        private TextControlBox tcb = null;

        public TabKey(TextControlBox tb)
        {
            tcb = tb;
        }

        //Tab-Key
        public void MoveTextWithTab_Back_WithoutSelection()
        {
            string linecontent = tcb.GetLineNumberContent[tcb.GetCurrentLineNumber - 1];

            if (linecontent.Contains(DefaultValues.DefaultTabSize) || linecontent.Contains("    "))
            {
                int currentcurpos = tcb.SelectionStart;
                tcb.SelectLine(tcb.GetCurrentLineNumber);

                string outsel = tcb.SelectedText;

                if (tcb.SelectedText.Contains(DefaultValues.DefaultTabSize))
                {
                    outsel = Extensions.StringBuilder.ReplaceFirstOccurenceInString(tcb.SelectedText, DefaultValues.DefaultTabSize, "");
                }
                else if (tcb.SelectedText.Contains("    "))
                {
                    outsel = Extensions.StringBuilder.ReplaceFirstOccurenceInString(tcb.SelectedText, "    ", "");
                }

                tcb.SelectedText = outsel;
                tcb.SetSelection(currentcurpos - 1, 0);
            }
        }
        public void MoveTextWithTab_Back_WithSelection()
        {
            if (tcb.SelectedText.Contains(DefaultValues.DefaultTabSize))
            {
                string[] AllLines = tcb.SelectedText.Split(new char[] { '\n', '\r' }, StringSplitOptions.None);
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                if (AllLines.Length == 1)
                {
                    sb.Append(Extensions.StringBuilder.ReplaceFirstOccurenceInString(tcb.GetLineNumberContent[tcb.GetCurrentLineNumber - 1], DefaultValues.DefaultTabSize, ""));
                    tcb.SelectLine(tcb.GetCurrentLineNumber);
                    tcb.SetSelection(tcb.SelectionStart, tcb.SelectionLenght - 1);
                    tcb.SelectedText = sb.ToString();
                }
                else
                {
                    for (int i = 0; i < AllLines.Length; i++)
                    {
                        if (tcb.SelectedText.Contains(DefaultValues.DefaultTabSize))
                        {
                            sb.Append(
                                Extensions.StringBuilder.ReplaceFirstOccurenceInString(AllLines[i], DefaultValues.DefaultTabSize, "") +
                                (i != AllLines.Length - 1 ? "\n" : ""));
                        }
                        else
                        {
                            sb.Append($"{AllLines[i]}\n");
                        }
                    }
                    tcb.SelectedText = sb.ToString();
                }
            }
        }
        public void MoveTextWithTab_Forward_WithoutSelection()
        {
            tcb.SelectedText = DefaultValues.DefaultTabSize;
            tcb.SetSelection(tcb.SelectionStart + DefaultValues.DefaultTabSize.Length, 0);
        }
        public void MoveTextWithTab_Forward_WithSelection()
        {
            string[] AllLines = tcb.SelectedText.Split(new char[] { '\n', '\r' }, StringSplitOptions.None);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            if (AllLines.Length == 1)
            {
                sb.Append(DefaultValues.DefaultTabSize + AllLines[0]);
            }
            else
            {
                for (int i = 0; i < AllLines.Length - 1; i++)
                {
                    sb.Append(DefaultValues.DefaultTabSize + AllLines[i] + (i != AllLines.Length - 1 ? "\n" : ""));
                }
            }
            tcb.SelectedText = sb.ToString();
        }
    }
}
