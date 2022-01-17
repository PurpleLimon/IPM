using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Windows;
using System.Text;
using Проекты_8_9_Классы.res;

namespace Проекты_8_9_Классы
{

    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {

        [DllImport("Shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

        //public string AppPath = new StringBuilder().Append(System.Reflection.Assembly.GetExecutingAssembly().Location.Trim().Reverse().SkipWhile((x) => x != '\\').Reverse().ToArray()).ToString();
        public string AppPath = System.AppDomain.CurrentDomain.BaseDirectory;
        public string SettingsPath = "bin//progset.ipmset";

        public Microsoft.Office.Interop.Word.Application wordApp = new Microsoft.Office.Interop.Word.Application();

        public SystemSettings.SysSettingsFile Settings;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            

            //MessageBox.Show(string.Join(" ", (e.Args.Length != 0)? e.Args: new string[] { "as", "asd"}));
            if (!System.IO.File.Exists(AppPath + SettingsPath))
            {
                System.IO.Directory.CreateDirectory(AppPath + "bin");
                SystemSettings.SaveFile(AppPath + SettingsPath, new SystemSettings.SysSettingsFile());

            }

            Settings = SystemSettings.LoadSysSettingFileFromBytes(AppPath + SettingsPath);
            if (Settings == null)
            {
                CloseApplication();
                return;
            }

            if (Settings.IsFirstlyOpened)
            {
#if !DEBUG 
#else
                if (!IsAssociated())
                {
                    Associate();
                }
                else
                {
                    ReAssociate();
                }
#endif
                System.IO.File.WriteAllBytes(AppPath + SettingsPath, new SystemSettings.SysSettingsFile() { IsFirstlyOpened = false }.ToBytes());

            }

            if (e.Args.Length == 0)
            {
                try
                {
                    MainWindow window = new MainWindow();
                    window.Height = SystemParameters.WorkArea.Height;
                    window.Width = SystemParameters.WorkArea.Width;
                    window.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    CloseApplication();
                    return;
                }
            }
            else
            {
                try
                {
                    MainWindow window = new MainWindow(string.Join(" ", e.Args));
                    window.Height = SystemParameters.WorkArea.Height;
                    window.Width = SystemParameters.WorkArea.Width;
                    window.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    CloseApplication();
                    return;
                }

            }

        }

        void App_Exit(object sender, ExitEventArgs e)
        {
            CloseApplication();
        }

        private void CloseApplication()
        {
            wordApp.Quit();
            Application.Current?.Shutdown();
            
        }

        private static bool IsAssociated()
        {
            return Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\.fsst", false) != null;
        }

        public static void ReAssociate()
        {
            RegistryKey appReg = Registry.CurrentUser.OpenSubKey("Software\\Classes\\Applications\\IPM.exe", true);
            appReg.OpenSubKey("shell\\open\\command", true).SetValue("", "\"" + System.Reflection.Assembly.GetExecutingAssembly().Location + "\" %1");
            appReg.OpenSubKey("shell\\edit\\command", true).SetValue("", "\"" + System.Reflection.Assembly.GetExecutingAssembly().Location + "\" %1");

            SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
        }

        public static void Associate()
        {
            RegistryKey fileReg = Registry.CurrentUser.CreateSubKey("Software\\Classes\\.fsst");
            RegistryKey appReg = Registry.CurrentUser.CreateSubKey("Software\\Classes\\Applications\\IPM.exe");
            RegistryKey appAssoc = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\.fsst");

            string iconPath = (Application.Current as App).AppPath + "icon.png";

            fileReg.CreateSubKey("DefaultIcon").SetValue("", iconPath);
            fileReg.CreateSubKey("PerceivedType").SetValue("", "Document");

            appReg.CreateSubKey("shell\\open\\command").SetValue("", "\"" + System.Reflection.Assembly.GetExecutingAssembly().Location + "\" %1");
            appReg.CreateSubKey("shell\\edit\\command").SetValue("", "\"" + System.Reflection.Assembly.GetExecutingAssembly().Location + "\" %1");
            appReg.CreateSubKey("DefaultIcon").SetValue("", iconPath);


            appAssoc.CreateSubKey("UserChoice").SetValue("Progid", "Applications\\IPM.exe");

            SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);

        }

    }
}
