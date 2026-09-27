namespace FantasyCritic.Lib.Patreon;

// The JSON:API shapes of the two Patreon responses we read, trimmed to the fields we request. Property names map to
// Patreon's snake_case through PatreonApiClient's serializer options.

internal record PatreonMembersPage(IReadOnlyList<PatreonResource> Data, IReadOnlyList<PatreonResource>? Included, PatreonLinks? Links)
{
    /// <summary>
    /// Joins each member to its user and tiers, which JSON:API puts in the page's included list. Members without a user
    /// cannot be linked to anyone, so they are left out.
    /// </summary>
    public IEnumerable<PatreonMember> ToMembers()
    {
        var included = (Included ?? []).ToDictionary(x => (x.Type, x.Id));
        foreach (var member in Data)
        {
            var userId = member.Relationships?.User?.Data?.Id;
            if (userId is null)
            {
                continue;
            }

            var fullName = included.GetValueOrDefault(("user", userId))?.Attributes?.FullName;
            var tierTitles = (member.Relationships?.CurrentlyEntitledTiers?.Data ?? [])
                .Select(tier => included[("tier", tier.Id)].Attributes?.Title)
                .OfType<string>()
                .ToList();
            yield return new PatreonMember(userId, fullName, tierTitles);
        }
    }
}

internal record PatreonResource(string Id, string Type, PatreonAttributes? Attributes, PatreonRelationships? Relationships);

// One record for the attributes of both included types: a tier has a title, a user has a full name.
internal record PatreonAttributes(string? Title, string? FullName);

internal record PatreonRelationships(PatreonToManyRelationship? CurrentlyEntitledTiers, PatreonToOneRelationship? User);

internal record PatreonToManyRelationship(IReadOnlyList<PatreonResourceIdentifier>? Data);

internal record PatreonToOneRelationship(PatreonResourceIdentifier? Data);

internal record PatreonResourceIdentifier(string Id, string Type);

internal record PatreonLinks(string? Next);

internal record PatreonTokenResponse(string AccessToken, string RefreshToken);
