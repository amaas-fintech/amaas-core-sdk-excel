using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMaaS.Core.Sdk.Excel.UI
{
    public interface ILoginView
    {
        IUserViewModel ViewModel { get; set; }
    }
}
