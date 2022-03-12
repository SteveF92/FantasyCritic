using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Duende.IdentityServer.Stores;

namespace FantasyCritic.MySQL.Entities.Identity
{
    internal class PersistedGrantFilterEntity
    {
        public PersistedGrantFilterEntity(PersistedGrantFilter domain)
        {
            SubjectId = domain.SubjectId;
            SessionId = domain.SessionId;
            ClientId = domain.ClientId;
            Type = domain.Type;
        }

        public string SubjectId { get; }
        public string SessionId { get; }
        public string ClientId { get; }
        public string Type { get; }
    }
}
