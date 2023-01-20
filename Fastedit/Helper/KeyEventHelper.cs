using System;
using Windows.System;
using Windows.UI.Core;

namespace Fastedit.Helper
{
    public class KeyEventHelper
    {
        public static void KeyboardCommand(KeyEventArgs PressedKey, VirtualKey KeyNeedForAction, Action action)
        {
            if (PressedKey.VirtualKey == KeyNeedForAction)
            {
                action?.Invoke();
            }
        }
    }
}
