using ExcelDna.Integration.CustomUI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AMaaS.Core.Sdk.Excel.UI
{
    [ComVisible(true)]
    public class ArgomiRibbon : ExcelRibbon
    {
        public void Login(IRibbonControl control)
        {
            var window = new Window
            {
                Title     = "Argomi Login",
                Content   = new LoginView(),
                MaxWidth  = 300,
                MaxHeight = 150
            };

            window.ShowDialog();
        }

        public override object LoadImage(string imageId)
        {
            try
            {
                var resourceNamespace = "AMaaS.Core.Sdk.Excel.Resources";
                var assembly          = Assembly.GetExecutingAssembly();
                var file              = assembly.GetManifestResourceStream(string.Join(".", resourceNamespace, imageId));
                return Image.FromStream(file);
            }
            catch
            {
                return base.LoadImage(imageId);
            }
        }
    }
}
