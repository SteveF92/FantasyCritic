using System;
using System.IO;

namespace FantasyCritic.UiTests;

public static class RepositoryRoot
{
    public static string Find()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "src", "FantasyCritic.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new InvalidOperationException($"No directory above {AppContext.BaseDirectory} holds src/FantasyCritic.slnx.");
    }
}
