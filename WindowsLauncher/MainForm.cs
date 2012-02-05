using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace TechnicLauncher
{
    public partial class Form1 : Form
    {
        public string LauncherURL = "https://raw.github.com/TechnicPack/Technic/master/";
        private const string LauncherBackupFile = Program.LaucherFile + ".bak";
        private const string LauncherTempFile = Program.LaucherFile + ".temp";
        private int _hashDownloadCount, _launcherDownloadCount;

        public bool IsAddressible(Uri uri)
        {
            try
            {
                using (var client = new MyClient())
                {
                    client.HeadOnly = true;
                    client.DownloadString(uri);
                    return true;
                }
            }
            catch (Exception loi)
            {
                
            }
            return false;
        }

        public Form1()
        {
            InitializeComponent();

            if (File.Exists(Program.LaucherFile))
            {
                DownloadHash();
            }
            else
            {
                DownloadLauncher();
            }
        }

        private void DownloadHash()
        {
            lblStatus.Text = @"Checking Launcher Version..";
            var versionCheck = new WebClient();
            versionCheck.DownloadStringCompleted += DownloadStringCompleted;
            var uri = new Uri(String.Format("{0}CHECKSUM.md5", LauncherURL));
            if (_hashDownloadCount < 3 && IsAddressible(uri))
            {
                _hashDownloadCount++;
                versionCheck.DownloadStringAsync(uri, Program.LaucherFile);
            }
            else
            {
                Program.RunLauncher();
                Close();
            }
        }

        private void DownloadLauncher()
        {
            lblStatus.Text = String.Format(@"Downloading Launcher ({0}/{1})..", _launcherDownloadCount, 3);
            
            if (_launcherDownloadCount < 3)
            {
                _launcherDownloadCount++;
                var wc = new WebClient();
                wc.DownloadProgressChanged += DownloadProgressChanged;
                wc.DownloadFileCompleted += DownloadFileCompleted;
                wc.DownloadFileAsync(new Uri(String.Format("{0}technic-launcher.jar", LauncherURL)), LauncherTempFile);
            }
            else
            {
                Program.RunLauncher();
                Close();
            }
        }

        void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                DownloadLauncher();
                return;
            }
            lblStatus.Text = @"Running Launcher..";
            pbStatus.Value = 100;

            if (File.Exists(LauncherBackupFile))
                File.Delete(LauncherBackupFile);
            if (File.Exists(Program.LaucherFile))
                File.Move(Program.LaucherFile, LauncherBackupFile);
            File.Move(LauncherTempFile, Program.LaucherFile);
            Program.RunLauncher();
            Close();
        }

        void DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                DownloadHash();
                return;
            }
            MD5 hash = new MD5CryptoServiceProvider();
            String md5, serverMD5 = null;
            var sb = new StringBuilder();
            using (var fs = File.Open(Program.LaucherFile, FileMode.Open, FileAccess.Read))
            {
                var md5Bytes = hash.ComputeHash(fs);
                foreach (byte hex in md5Bytes)
                    sb.Append(hex.ToString("x2"));
                md5 = sb.ToString().ToLowerInvariant();
            }

            var lines = e.Result.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None);
            foreach (var line in lines)
            {
                if (!line.Contains("technic-launcher.jar")) continue;
                serverMD5 = line.Split('|')[0].ToLowerInvariant();
                break;
            }

            if (serverMD5 != null && serverMD5.Equals(md5)) {
                Program.RunLauncher();
                Close();
            }
            else
            {
                DownloadLauncher();
            }
        }

        void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            lblStatus.Text = String.Format("Downloaded {0}% of launcher..", e.ProgressPercentage);
            pbStatus.Value = e.ProgressPercentage;
        }
    }
}
