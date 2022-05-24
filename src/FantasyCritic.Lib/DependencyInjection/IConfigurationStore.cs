using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.DependencyInjection;
public interface IConfigurationStore
{
    string GetConfigValue(string name);
}
