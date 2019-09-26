using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Web.Middleware
{
    public class SpaFallbackOptions
    {
        public SpaFallbackOptions()
        {
            this.ApiPathPrefix = "/api";
            this.RewritePath = "/";
        }
        public string ApiPathPrefix { get; set; }
        public string RewritePath { get; set; }
    }
}
