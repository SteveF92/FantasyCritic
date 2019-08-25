using FantasyCritic.Lib.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace FantasyCritic.Lib.Services
{
    public class RoyaleService
    {
        private readonly IRoyaleRepo _royaleRepo;

        public RoyaleService(IRoyaleRepo royaleRepo)
        {
            _royaleRepo = royaleRepo;
        }
    }
}
