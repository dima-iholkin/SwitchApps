using System;
using System.Runtime.InteropServices;
using Serilog.Core;

// Thanks to this honorable man we have this working https://gist.github.com/skepticMike/caa8a43db86f6d027e400ad9196e100a
// And thanks to this forum too https://www.tenforums.com/tutorials/6377-change-visual-effects-settings-windows-10-a-6.html

namespace SwitchApps_Library.MenuAnimation
{
    internal class MenuAnimationUtility
    {
        // Init:

        public MenuAnimationUtility(Logger logger)
        {
            this._logger = logger;
        }

        private readonly Logger _logger;

        // External function signatures:

        // Used to set the new Menu Animation value.
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SystemParametersInfo(uint uiAction, uint uiParam, bool pvParam, uint fWinIni);

        // Used to get the current Menu Animation value.
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref bool pvParam, uint fWinIni);

        // Public methods:

        public bool? GetMenuAnimation()
        {
            const uint SPI_GETMENUANIMATION = 0x1002;

            bool menuAnimationIsEnabled = false;

            bool exitValue = SystemParametersInfo(SPI_GETMENUANIMATION, 0, ref menuAnimationIsEnabled, 0);

            if (exitValue == false)
            {
                this._logger.Error("Win32 function call failed with error code {Win32ErrorCode}.", Marshal.GetLastWin32Error());
                return null;
            }

            this._logger.Information("Menu animation is {MenuAnimationIsEnabled}.", menuAnimationIsEnabled);
            return menuAnimationIsEnabled;
        }

        public void SetMenuAnimation(bool newValue)
        {
            const uint SPIF_UPDATEINIFILE = 0x01;
            const uint SPIF_SENDCHANGE = 0x02;
            const uint SPI_SETMENUANIMATION = 0x1003;

            bool exitValue = SystemParametersInfo(
                SPI_SETMENUANIMATION, 0, newValue, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE
            );

            if (exitValue == false)
            {
                this._logger.Error("Win32 function call failed with error code {Win32ErrorCode}.", Marshal.GetLastWin32Error());
            }
            else
            {
                this._logger.Information("Menu animation set to {MenuAnimationNewValue}.", newValue);
            }
        }
    }
}