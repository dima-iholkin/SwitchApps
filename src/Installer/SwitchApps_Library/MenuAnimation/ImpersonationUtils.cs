using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Serilog.Core;

namespace SwitchApps_Library.MenuAnimation
{
    internal static class ImpersonationUtils
    {
        // Const:

        private const int TOKEN_QUERY = 0x0008;
        private const int TOKEN_DUPLICATE = 0x0002;
        private const int TOKEN_ASSIGN_PRIMARY = 0x0001;
        private const int TOKEN_IMPERSONATE = 0x0004;
        private const int TOKEN_QUERY_SOURCE = 0x0010;
        private const int TOKEN_ADJUST_PRIVILEGES = 0x0020;
        private const int TOKEN_ADJUST_GROUPS = 0x0040;
        private const int TOKEN_ADJUST_DEFAULT = 0x0080;
        private const int TOKEN_ADJUST_SESSIONID = 0x0100;
        private const int STANDARD_RIGHTS_REQUIRED = 0x000F0000;

        private const int TOKEN_ALL_ACCESS =
            STANDARD_RIGHTS_REQUIRED |
            TOKEN_ASSIGN_PRIMARY |
            TOKEN_DUPLICATE |
            TOKEN_IMPERSONATE |
            TOKEN_QUERY |
            TOKEN_QUERY_SOURCE |
            TOKEN_ADJUST_PRIVILEGES |
            TOKEN_ADJUST_GROUPS |
            TOKEN_ADJUST_DEFAULT |
            TOKEN_ADJUST_SESSIONID;

        [StructLayout(LayoutKind.Sequential)]
        private struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        private enum SECURITY_IMPERSONATION_LEVEL
        {
            SecurityAnonymous,
            SecurityIdentification,
            SecurityImpersonation,
            SecurityDelegation
        }

        private enum TOKEN_TYPE
        {
            TokenPrimary = 1,
            TokenImpersonation
        }

        // External calls:

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool DuplicateTokenEx(
            IntPtr hExistingToken, int dwDesiredAccess, ref SECURITY_ATTRIBUTES lpThreadAttributes,
            int ImpersonationLevel, int dwTokenType, ref IntPtr phNewToken
        );

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool OpenProcessToken(IntPtr ProcessHandle, int DesiredAccess, ref IntPtr TokenHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        // Public methods:

        public static WindowsImpersonationContext ImpersonateLoginUser(Logger logger)
        {
            try
            {
                Process process = Process.GetProcessesByName("explorer").FirstOrDefault();

                IntPtr userToken = ImpersonationUtils.GetPrimaryToken(process);

                if (userToken == IntPtr.Zero)
                {
                    logger.Warning("UserToken == Zero. Failed at impersonating the login user.");
                }

                WindowsIdentity identity = new WindowsIdentity(userToken);

                return identity.Impersonate();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed at impersonating the login user.");

                return WindowsIdentity.GetCurrent().Impersonate();
            }
        }

        // Private methods:

        internal static IntPtr GetPrimaryToken(Process process)
        {
            IntPtr token = IntPtr.Zero;
            IntPtr primaryToken = IntPtr.Zero;

            bool openProcessTokenSuccess = OpenProcessToken(process.Handle, TOKEN_DUPLICATE, ref token);

            if (openProcessTokenSuccess == false)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "OpenProcessToken failed");
            }

            SECURITY_ATTRIBUTES sa = new SECURITY_ATTRIBUTES();
            sa.nLength = Marshal.SizeOf(sa);

            bool duplicateTokenSuccess = DuplicateTokenEx(
                token, TOKEN_ALL_ACCESS, ref sa, (int)SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation,
                (int)TOKEN_TYPE.TokenPrimary, ref primaryToken
            );

            if (duplicateTokenSuccess == false)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "DuplicateTokenEx failed");
            }

            CloseHandle(token);

            return primaryToken;
        }
    }
}