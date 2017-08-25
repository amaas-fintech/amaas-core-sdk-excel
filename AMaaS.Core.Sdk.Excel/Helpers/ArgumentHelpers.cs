using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMaaS.Core.Sdk.Excel.Helpers
{
    public static class ArgumentHelpers
    {
        public static bool MatchAll(this string argument) => string.IsNullOrWhiteSpace(argument) || argument.Trim() == "*";
    }
}
