﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain
{
    public class MasterGameTag
    {
        public MasterGameTag(string name, string readableName, string shortName, MasterGameTagType tagType, bool hasCustomCode, string description, IEnumerable<string> examples, string badgeColor)
        {
            Name = name;
            ReadableName = readableName;
            ShortName = shortName;
            TagType = tagType;
            HasCustomCode = hasCustomCode;
            Description = description;
            Examples = examples.ToList();
            BadgeColor = badgeColor;
        }

        public string Name { get; }
        public string ReadableName { get; }
        public string ShortName { get; }
        public MasterGameTagType TagType { get; }
        public bool HasCustomCode { get; }
        public string Description { get; }
        public IReadOnlyList<string> Examples { get; }
        public string BadgeColor { get; }
    }
}
