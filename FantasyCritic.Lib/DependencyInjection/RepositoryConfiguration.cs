using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace FantasyCritic.Lib.DependencyInjection
{
    public record RepositoryConfiguration(string ConnectionString, IClock Clock);
}
