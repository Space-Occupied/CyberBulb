using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Principal;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Diagnostics.Eventing.Reader;
using System.Reflection;

namespace CyberBulb
{
    public partial class Form1 : Form
    {
        bool isAdmin;
        bool perfectDone;
        [DllImport("Wtsapi32.dll", SetLastError = true)]
        static extern bool WTSLogoffSession(IntPtr hServer, int SessionId, bool bWait);

        [DllImport("Kernel32.dll")]
        static extern IntPtr WTSGetActiveConsoleSessionId();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            isAdmin = IsRunningAsAdmin();
            if (isAdmin)
            {
                MessageBox.Show("本程序已获得管理员权限，祝您玩的开心！");
            }
            else
            {
                MessageBox.Show("建议以管理员身份运行本程序，体验感更佳！");
            }
        }

        private bool IsRunningAsAdmin()
        {
            WindowsIdentity currentIdentity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(currentIdentity);

            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            if(setResolution() && setScaling())
            {
                if (!perfectDone)
                {
                    MessageBox.Show("某些显示器不支持的设置没有生效");
                }
                if (!Log_Out())
                {
                    MessageBox.Show("设置修改完毕！注销后生效");
                }
            }else
            {
                if(isAdmin)
                {
                    MessageBox.Show("出现未知错误！请尝试重启计算机");
                }else
                {
                    MessageBox.Show("某些设置可能未生效，请以管理员身份运行本程序。");
                }
            }
        }

        private bool setResolution()
        {
            int newWidth = 800; // 替换为你想要设置的新宽度
            int newHeight = 600; // 替换为你想要设置的新高度
            bool ret = false;
            perfectDone = true;
            
            Screen screen;
            for (int i = 0; i < Screen.AllScreens.Length; i++)
            {
                screen = Screen.AllScreens[i];
                try
                {
                    // 更改分辨率
                    if (screen != null)
                    {
                        DEVMODE devMode = new DEVMODE();
                        devMode.dmSize = (short)System.Runtime.InteropServices.Marshal.SizeOf(devMode);
                        devMode.dmPelsWidth = newWidth;
                        devMode.dmPelsHeight = newHeight;
                        devMode.dmDisplayOrientation = 2;
                        devMode.dmFields = DEVMODE_Flags.DM_PELSWIDTH | DEVMODE_Flags.DM_PELSHEIGHT;

                        int res = User_32.ChangeDisplaySettingsEx(screen.DeviceName, ref devMode, IntPtr.Zero, ChangeDisplaySettingsFlags.CDS_UPDATEREGISTRY, IntPtr.Zero);
                        if (res == 0)
                        {
                            ret = true;
                        }else
                        {
                            perfectDone = false;
                        }

                        devMode.dmFields = DEVMODE_Flags.DM_DISPLAYORIENTATION;
                        res = User_32.ChangeDisplaySettingsEx(screen.DeviceName, ref devMode, IntPtr.Zero, ChangeDisplaySettingsFlags.CDS_UPDATEREGISTRY, IntPtr.Zero);
                        if(res != 0)
                        {
                            perfectDone = false;
                        }
                    }
                }
                catch (Exception)
                {
                    perfectDone = false;
                }
            }
            return ret;
        }

        private bool setScaling()
        {
            try
            {
                // 修改注册表项的值
                Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "LogPixels", 480, RegistryValueKind.DWord);// DPI:480即为自定义缩放500%
                Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "Win8DpiScaling", 1, RegistryValueKind.DWord);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private bool Log_Out()
        {
            IntPtr sessionIdPtr = WTSGetActiveConsoleSessionId();
            int sessionId = sessionIdPtr.ToInt32();

            // 注销当前会话
            return WTSLogoffSession(IntPtr.Zero, sessionId, false);
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            int nowVal = ((TrackBar)sender).Value;
            if(nowVal == 100)
            {
                button1.Enabled = true;
            }else
            {
                button1.Enabled = false;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINTL
    {
        long x;
        long y;
    }

    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi)]
    public struct DEVMODE
    {
        public const int CCHDEVICENAME = 32;
        public const int CCHFORMNAME = 32;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
        [System.Runtime.InteropServices.FieldOffset(0)]
        public string dmDeviceName;
        [System.Runtime.InteropServices.FieldOffset(32)]
        public Int16 dmSpecVersion;
        [System.Runtime.InteropServices.FieldOffset(34)]
        public Int16 dmDriverVersion;
        [System.Runtime.InteropServices.FieldOffset(36)]
        public Int16 dmSize;
        [System.Runtime.InteropServices.FieldOffset(38)]
        public Int16 dmDriverExtra;
        [System.Runtime.InteropServices.FieldOffset(40)]
        public int dmFields;

        [System.Runtime.InteropServices.FieldOffset(44)]
        Int16 dmOrientation;
        [System.Runtime.InteropServices.FieldOffset(46)]
        Int16 dmPaperSize;
        [System.Runtime.InteropServices.FieldOffset(48)]
        Int16 dmPaperLength;
        [System.Runtime.InteropServices.FieldOffset(50)]
        Int16 dmPaperWidth;
        [System.Runtime.InteropServices.FieldOffset(52)]
        Int16 dmScale;
        [System.Runtime.InteropServices.FieldOffset(54)]
        Int16 dmCopies;
        [System.Runtime.InteropServices.FieldOffset(56)]
        Int16 dmDefaultSource;
        [System.Runtime.InteropServices.FieldOffset(58)]
        Int16 dmPrintQuality;

        [System.Runtime.InteropServices.FieldOffset(44)]
        public POINTL dmPosition;
        [System.Runtime.InteropServices.FieldOffset(52)]
        public Int32 dmDisplayOrientation;
        [System.Runtime.InteropServices.FieldOffset(56)]
        public Int32 dmDisplayFixedOutput;

        [System.Runtime.InteropServices.FieldOffset(60)]
        public short dmColor;
        [System.Runtime.InteropServices.FieldOffset(62)]
        public short dmDuplex;
        [System.Runtime.InteropServices.FieldOffset(64)]
        public short dmYResolution;
        [System.Runtime.InteropServices.FieldOffset(66)]
        public short dmTTOption;
        [System.Runtime.InteropServices.FieldOffset(68)]
        public short dmCollate;
        [System.Runtime.InteropServices.FieldOffset(72)]
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
        public string dmFormName;
        [System.Runtime.InteropServices.FieldOffset(102)]
        public Int16 dmLogPixels;
        [System.Runtime.InteropServices.FieldOffset(104)]
        public Int32 dmBitsPerPel;
        [System.Runtime.InteropServices.FieldOffset(108)]
        public Int32 dmPelsWidth;
        [System.Runtime.InteropServices.FieldOffset(112)]
        public Int32 dmPelsHeight;
        [System.Runtime.InteropServices.FieldOffset(116)]
        public Int32 dmDisplayFlags;
        [System.Runtime.InteropServices.FieldOffset(116)]
        public Int32 dmNup;
        [System.Runtime.InteropServices.FieldOffset(120)]
        public Int32 dmDisplayFrequency;
    }

    [System.Flags]
    public enum ChangeDisplaySettingsFlags : int
    {
        CDS_NONE = 0,
        CDS_UPDATEREGISTRY = 0x00000001,
        CDS_TEST = 0x00000002,
        CDS_FULLSCREEN = 0x00000004,
        CDS_GLOBAL = 0x00000008,
        CDS_SET_PRIMARY = 0x00000010,
        CDS_VIDEOPARAMETERS = 0x00000020,
        CDS_ENABLE_UNSAFE_MODES = 0x00000100,
        CDS_DISABLE_UNSAFE_MODES = 0x00000200,
        CDS_RESET = 0x40000000,
        CDS_RESET_EX = 0x20000000,
        CDS_NORESET = 0x10000000
    }

    public static class DEVMODE_Flags
    {
        public const int DM_ORIENTATION = 0x00000001;
        public const int DM_PAPERSIZE = 0x00000002;
        public const int DM_PAPERLENGTH = 0x00000004;
        public const int DM_PAPERWIDTH = 0x00000008;
        public const int DM_SCALE = 0x00000010;
        public const int DM_POSITION = 0x00000020;
        public const int DM_NUP = 0x00000040;
        public const int DM_DISPLAYORIENTATION = 0x00000080;
        public const int DM_COPIES = 0x00000100;
        public const int DM_DEFAULTSOURCE = 0x00000200;
        public const int DM_PRINTQUALITY = 0x00000400;
        public const int DM_COLOR = 0x00000800;
        public const int DM_DUPLEX = 0x00001000;
        public const int DM_YRESOLUTION = 0x00002000;
        public const int DM_TTOPTION = 0x00004000;
        public const int DM_COLLATE = 0x00008000;
        public const int DM_FORMNAME = 0x00010000;
        public const int DM_LOGPIXELS = 0x00020000;
        public const int DM_BITSPERPEL = 0x00040000;
        public const int DM_PELSWIDTH = 0x00080000;
        public const int DM_PELSHEIGHT = 0x00100000;
        public const int DM_DISPLAYFLAGS = 0x00200000;
        public const int DM_DISPLAYFREQUENCY = 0x00400000;
        public const int DM_POSITIONFLAGS = 0x00800000;
        public const int DM_PANNINGWIDTH = 0x01000000;
        public const int DM_PANNINGHEIGHT = 0x02000000;
        public const int DM_DISPLAYFIXEDOUTPUT = 0x04000000;
    }

    public class User_32
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int ChangeDisplaySettingsEx(string lpszDeviceName, ref DEVMODE lpDevMode, IntPtr hwnd, ChangeDisplaySettingsFlags dwflags, IntPtr lParam);
    }
}