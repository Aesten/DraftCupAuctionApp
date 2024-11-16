using System;
using System.IO;
using System.Windows.Forms;

namespace AuctionApp
{
    internal static class Program
    {
        public static readonly string AppDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DraftCupPicker");
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {   
            if (!Directory.Exists(AppDir))
            {
                Directory.CreateDirectory(AppDir);
            }
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}