using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace TechnicLauncher
{
    static class Program
    {
        public const string LaucherFile = "technic-launcher.jar";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        private static String GetJavaInstallationPath()
        {
            const string javaKey = "SOFTWARE\\JavaSoft\\Java Runtime Environment";
            using (var baseKey = Registry.LocalMachine.OpenSubKey(javaKey))
            {
                if (baseKey != null)
                {
                    String currentVersion = baseKey.GetValue("CurrentVersion").ToString();
                    using (var homeKey = baseKey.OpenSubKey(currentVersion))
                    {
                        if (homeKey != null)
                        {
                            var home = homeKey.GetValue("JavaHome");
                            return home.ToString();
                        }
                    }
                }
            }
            return null;
        }

        public static void RunLauncher()
        {
            var java =  GetJavaInstallationPath();//Environment.GetEnvironmentVariable("JAVA_HOME") ??
            if (java == null)
            {
                MessageBox.Show(@"Can't find java directory.");
            }
            else
            {   
                var info = new ProcessStartInfo
                               {
                                   CreateNoWindow = true,
                                   WorkingDirectory = Application.StartupPath,
                                   FileName = Path.Combine(java, "bin\\java.exe"),
                                   Arguments = String.Format("-jar {0}", LaucherFile),
                                   UseShellExecute = false
                               };
                Process.Start(info);
            }
            Application.Exit();
        }

    }
}
