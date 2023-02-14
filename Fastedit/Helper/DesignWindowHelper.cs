using Fastedit.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml.Hosting;
using Windows.UI;
using System.Diagnostics;
using System.IO;
using Fastedit.Settings;
using Windows.UI.Xaml.Media;
using Fastedit.Dialogs;
using Microsoft.UI.Xaml.Controls;
using Fastedit.Controls;

namespace Fastedit.Helper
{
    internal class DesignWindowHelper
    {
        public static async void EditDesign(string designName)
        {
            var design = DesignHelper.GetDesignFromFile(Path.Combine(DefaultValues.DesignPath, designName));

            if (design == null)
                return;

            var window = await ShowWindow(design, designName);
            if (window == null)
                return;
        }

        private static async Task<AppWindow> ShowWindow(FasteditDesign design, string designName)
        {
            var window = await AppWindow.TryCreateAsync();
            window.TitleBar.ExtendsContentIntoTitleBar = true;
            window.TitleBar.ButtonBackgroundColor = Colors.Transparent;

            ElementCompositionPreview.SetAppWindowContent(window, new DesignEditor(window, design, designName));

            window.CloseRequested += Window_CloseRequested;
            if (await window.TryShowAsync())
            {
                return window;
            }
            return null;
        }

        private static async void Window_CloseRequested(AppWindow sender, AppWindowCloseRequestedEventArgs args)
        {
            var def = args.GetDeferral();
            //Ask save the design when it was modified
            if (ElementCompositionPreview.GetAppWindowContent(sender) is DesignEditor designEditor
                && designEditor.NeedSave)
            {
                args.Cancel = true;
                var res = await AskSaveDesignDialog.Show(designEditor);
                switch (res)
                {
                    case Windows.UI.Xaml.Controls.ContentDialogResult.Primary:
                        if (await designEditor.SaveDesign())
                        {
                            InfoMessages.SaveDesignSucceed();
                            args.Cancel = false;
                            def.Complete();
                            return;
                        }
                        else
                            InfoMessages.SaveDesignError();
                        break;
                    case Windows.UI.Xaml.Controls.ContentDialogResult.Secondary:
                        args.Cancel = false;
                        break;
                }
            }
            def.Complete();
        }
    }
}
