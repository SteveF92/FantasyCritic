using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace FantasyCritic.UiTests;

/// <summary>
/// Every distinct console warning the run saw, with the tests and pages it appeared on. Warnings don't fail tests.
/// </summary>
public sealed class ConsoleWarningsReport
{
    private readonly Lock _lock = new();
    private readonly Dictionary<string, SortedSet<string>> _sightings = [];

    public void Add(string message, string test, string page)
    {
        lock (_lock)
        {
            if (!_sightings.TryGetValue(message, out var where))
            {
                where = [];
                _sightings[message] = where;
            }

            where.Add($"{test} on {page}");
        }
    }

    public string ToMarkdown()
    {
        lock (_lock)
        {
            var markdown = new StringBuilder("# Console warnings\n\n");
            if (_sightings.Count == 0)
            {
                return markdown.Append("None.\n").ToString();
            }

            foreach (var (message, where) in _sightings.OrderByDescending(sighting => sighting.Value.Count).ThenBy(sighting => sighting.Key))
            {
                markdown.Append($"## {where.Count} × {message.Split('\n')[0]}\n\n");
                if (message.Contains('\n'))
                {
                    markdown.Append($"```\n{message}\n```\n\n");
                }

                foreach (var sighting in where)
                {
                    markdown.Append($"- {sighting}\n");
                }

                markdown.Append('\n');
            }

            return markdown.ToString();
        }
    }
}
