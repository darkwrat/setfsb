using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using OpenLibSys;

namespace SetFSB {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var ols = InitIO();
            if(ols == null) return;
            
            var smb = new smBus(ols);
            //smb.enable_smbus();
            var plls = new List<Pll>(){new ICS9LPRS365BGLF(smb), new ICS9LPRS355(smb)};
            
            
            Application.Run(new Form1(plls));
        }

        public static Ols  InitIO(){
        var ols = new Ols();

        // Check support library sutatus
        switch (ols.GetStatus()){
            case (uint) Ols.Status.NO_ERROR:
                break;
            case (uint) Ols.Status.DLL_NOT_FOUND:
                MessageBox.Show("Status Error!! DLL_NOT_FOUND");
                return null;
            case (uint) Ols.Status.DLL_INCORRECT_VERSION:
                MessageBox.Show("Status Error!! DLL_INCORRECT_VERSION");
                return null;
            case (uint) Ols.Status.DLL_INITIALIZE_ERROR:
                MessageBox.Show("Status Error!! DLL_INITIALIZE_ERROR");
                return null;
        }

        // Check WinRing0 status
        switch (ols.GetDllStatus()){
            case (uint) Ols.OlsDllStatus.OLS_DLL_NO_ERROR:
                break;
            case (uint) Ols.OlsDllStatus.OLS_DLL_DRIVER_NOT_LOADED:
                MessageBox.Show("DLL Status Error!! OLS_DRIVER_NOT_LOADED");
                return null;
            case (uint) Ols.OlsDllStatus.OLS_DLL_UNSUPPORTED_PLATFORM:
                MessageBox.Show("DLL Status Error!! OLS_UNSUPPORTED_PLATFORM");
                return null;
            case (uint) Ols.OlsDllStatus.OLS_DLL_DRIVER_NOT_FOUND:
                MessageBox.Show("DLL Status Error!! OLS_DLL_DRIVER_NOT_FOUND");
                return null;
            case (uint) Ols.OlsDllStatus.OLS_DLL_DRIVER_UNLOADED:
                MessageBox.Show("DLL Status Error!! OLS_DLL_DRIVER_UNLOADED");
                return null;
            case (uint) Ols.OlsDllStatus.OLS_DLL_DRIVER_NOT_LOADED_ON_NETWORK:
                MessageBox.Show("DLL Status Error!! DRIVER_NOT_LOADED_ON_NETWORK");
                return null;
            case (uint) Ols.OlsDllStatus.OLS_DLL_UNKNOWN_ERROR:
                MessageBox.Show("DLL Status Error!! OLS_DLL_UNKNOWN_ERROR");
                return null;
        }
        return ols;
    }

    }
}
