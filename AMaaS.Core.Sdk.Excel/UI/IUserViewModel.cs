using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMaaS.Core.Sdk.Excel.UI
{
    public interface IUserViewModel
    {
        string Username { get; set; }
        string Password { get; set; }
    }
}
