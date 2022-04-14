using System;
using Windows.System;

namespace Fastedit.Extensions
{
    public class KeyboardCommands
    {
        public static void KeyboardCommand<T>(VirtualKey PressedKey, VirtualKey KeyNeedForAction, Action<T> action, T args)
        {
            if (PressedKey == KeyNeedForAction)
            {
                action?.Invoke(args);
            }
        }

        public static void KeyboardCommand(VirtualKey PressedKey, VirtualKey KeyNeedForAction, Action action)
        {
            if (PressedKey == KeyNeedForAction)
            {
                action?.Invoke();
            }
        }

    }
}
