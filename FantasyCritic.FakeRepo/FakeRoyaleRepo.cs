using FantasyCritic.Lib.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace FantasyCritic.FakeRepo
{
    public class FakeRoyaleRepo : IRoyaleRepo
    {
        private readonly FakeFantasyCriticUserStore _userStore;
        private readonly FakeMasterGameRepo _fakeMasterGameRepo;

        public FakeRoyaleRepo(FakeFantasyCriticUserStore userStore, FakeMasterGameRepo fakeMasterGameRepo)
        {
            _userStore = userStore;
            _fakeMasterGameRepo = fakeMasterGameRepo;
        }
    }
}
