﻿using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Vocup.Forms;
using Vocup.Models;
using Vocup.Properties;
using Vocup.Util;

namespace Vocup
{
    static class Program
    {
        /// <summary>
        /// The main entry-point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            // Prevents the installer from executing while the program is running
            new Mutex(false, AppInfo.ProductName, out _);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            SplashScreen splash = new SplashScreen();
            splash.Show();
            Application.DoEvents();

            if (Settings.Default.StartupCounter == 0)
                Settings.Default.Upgrade(); // Keep old settings with new version
            // Warning: Unsaved changes are overridden

            SetCulture();
            if (!CreateVhfFolder() || !CreateVhrFolder())
            {
                Application.Exit();
                return;
            }

            Settings.Default.StartupCounter++;
            Settings.Default.Save();
            Application.DoEvents();

            Form form;

            if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
            {
                FileInfo info = new FileInfo(args[0]);
                if (info.Extension == ".vhf")
                {
                    var mainForm = new MainForm();
                    mainForm.ReadFile(info.FullName);
                    form = mainForm;
                }
                else if (info.Extension == ".vdp")
                {
                    form = new RestoreBackup(info.FullName);
                }
                else
                {
                    form = new MainForm();
                    MessageBox.Show(string.Format(Messages.OpenUnknownFile, info.FullName),
                        Messages.OpenUnknownFileT, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else if (Settings.Default.StartScreen == (int)StartScreen.LastFile && File.Exists(Settings.Default.LastFile))
            {
                var mainForm = new MainForm();
                mainForm.ReadFile(Settings.Default.LastFile);
                form = mainForm;
            }
            else
            {
                form = new MainForm();
            }

            Application.DoEvents();

            splash.Close();
            Application.Run(form);
        }

        /// <summary>
        /// Checks the currently configured folder for .vhf files and creates it if not existing.
        /// </summary>
        internal static bool CreateVhfFolder()
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

            if (string.IsNullOrWhiteSpace(Settings.Default.VhfPath))
            {
                Settings.Default.VhfPath = folder;
            }
            else if (!Directory.Exists(Settings.Default.VhfPath))
            {
                if (MessageBox.Show(Messages.VhfPathNotFound, Messages.VhfPathNotFoundT, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                {
                    Settings.Default.VhfPath = folder;
                }
                else return false;
            }

            return true;
        }

        /// <summary>
        /// Checks the currently configured folder for .vhr files and creates it if not existing.
        /// </summary>
        internal static bool CreateVhrFolder()
        {
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                AppInfo.ProductName);

            if (string.IsNullOrWhiteSpace(Settings.Default.VhrPath))
            {
                Directory.CreateDirectory(folder);
                Settings.Default.VhrPath = folder;
            }
            else if (!Directory.Exists(Settings.Default.VhrPath))
            {
                if (MessageBox.Show(Messages.VhrPathNotFound, Messages.VhrPathNotFoundT, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                {
                    Directory.CreateDirectory(folder);
                    Settings.Default.VhrPath = folder;
                }
                else return false;
            }

            return true;
        }

        internal static void SetCulture()
        {
            if (!string.IsNullOrWhiteSpace(Settings.Default.OverrideCulture))
            {
                try
                {
                    CultureInfo culture = new CultureInfo(Settings.Default.OverrideCulture);
                    CultureInfo.DefaultThreadCurrentCulture = culture;
                    CultureInfo.DefaultThreadCurrentUICulture = culture;
                }
                catch (CultureNotFoundException)
                {
                    Settings.Default.OverrideCulture = "";
                }
            }
        }
    }
}
