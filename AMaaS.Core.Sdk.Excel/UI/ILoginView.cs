using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AMaaS.Core.Sdk.Excel.UI
{
    public interface ILoginView
    {
        Window ParentWindow { get; set; }
        IUserViewModel ViewModel { get; set; }
    }
}
