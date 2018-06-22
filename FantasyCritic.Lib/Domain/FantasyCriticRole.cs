using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace FantasyCritic.Lib.Domain
{
    public class FantasyCriticRole
    {
        public FantasyCriticRole(int roleID, string name, string normalizedName)
        {
            RoleID = roleID;
            Name = name;
            NormalizedName = normalizedName;
        }

        public int RoleID { get; set; }

        public string Name { get; set; }

        public string NormalizedName { get; set; }
    }
}
