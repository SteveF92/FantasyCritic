using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Web.Extensions
{
    public class RazorExtensions
    {
        public static bool IsReleaseBuild()
        {
#if DEBUG
            return false;
#else
            return true;
#endif
        }
    }
}
