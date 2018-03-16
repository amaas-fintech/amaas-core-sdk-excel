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
                Width  = 300,
                Height = 200,
                MaxWidth = 800,
                MaxHeight = 800,
                ResizeMode = ResizeMode.CanResize
            };

            window.Content = new LoginView { ParentWindow = window };
            window.ShowDialog();
        }

        public void OnLoad(IRibbonUI sender)
        {
            AddinContext.Excel.Ribbon = sender;
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

        public int GetItemCount(IRibbonControl control)
        {
            return AddinContext.UserContext?.AssetManagerParties?.Count() ?? 0;
        }

        public bool GetEnabled(IRibbonControl control)
        {
            var parties = AddinContext.UserContext?.AssetManagerParties ?? null;
            return parties?.Count() > 0;
        }

        public string GetItemId(IRibbonControl control, int index)
        {
            return AddinContext.UserContext?.AssetManagerParties?.ToList()[index].PartyId ?? string.Empty;
        }

        public string GetSelectedItemId(IRibbonControl control)
        {
            return AddinContext.UserContext?.SelectedAssetManagerParty?.PartyId ?? string.Empty;
        }

        public string GetItemLabel(IRibbonControl control, int index)
        {
            return AddinContext.UserContext?.AssetManagerParties?.ToList()[index].DisplayName ?? string.Empty;
        }

        public void SaveChoice(IRibbonControl control, string selectedId, int selectedIndex)
        {
            var selectedParty = AddinContext.UserContext?.AssetManagerParties?.ToList()[selectedIndex];
            if (selectedParty != null)
            {
                AddinContext.UserContext.SelectedAssetManagerParty = selectedParty;
            }
        }
    }
}
