using System;
using System.Collections.Generic;
using System.Text;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.MySQL.Entities
{
    internal class FantasyCriticRoleEntity
    {
        public FantasyCriticRoleEntity()
        {

        }

        public FantasyCriticRoleEntity(FantasyCriticRole role)
        {
            RoleID = role.RoleID;
            Name = role.Name;
            NormalizedName = role.NormalizedName;
        }

        public int RoleID { get; set; }
        public string Name { get; set; }
        public string NormalizedName { get; set; }


        public FantasyCriticRole ToDomain()
        {
            FantasyCriticRole domain = new FantasyCriticRole(RoleID, Name, NormalizedName);
            return domain;
        }
    }
}
