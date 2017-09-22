using System;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Windows.Forms;
using eZstd.MarshalReflection;
using Microsoft.Win32;

namespace eZcad.SubgradeQuantity.SQControls
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new CadAddinSetup());
        }

    }
}