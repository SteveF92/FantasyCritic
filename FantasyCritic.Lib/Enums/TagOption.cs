using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Enums
{
    public class TagOption : TypeSafeEnum<TagOption>
    {
        // Define values here.
        public static readonly TagOption Banned = new TagOption("Banned");
        public static readonly TagOption Required = new TagOption("Required");

        // Constructor is private: values are defined within this class only!
        private TagOption(string value)
            : base(value)
        {

        }

        public override string ToString() => Value;
    }
}
