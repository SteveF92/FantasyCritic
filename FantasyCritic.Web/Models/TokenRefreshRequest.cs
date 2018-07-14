using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Web.Models
{
    public class TokenRefreshRequest
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
}
