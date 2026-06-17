using System;
using System.Collections.Generic;

namespace FantasyCritic.IntegrationTests.ApiModels;

/// <summary>
/// JSON shapes for Royale API responses. Web ViewModels in this area are built from
/// domain types and are not suitable for Newtonsoft deserialization in tests.
/// Request types remain <c>FantasyCritic.Web.Models.Requests.Royale</c>.
/// </summary>
internal sealed class RoyaleYearQuarterJson
{
    public int Year { get; set; }
    public int Quarter { get; set; }
    public bool OpenForPlay { get; set; }
    public bool Finished { get; set; }
}

internal sealed class RoyalePublisherJson
{
    public Guid PublisherID { get; set; }
    public string PublisherName { get; set; } = "";
    public string? PublisherIcon { get; set; }
    public string? PublisherSlogan { get; set; }
    public RoyaleYearQuarterJson YearQuarter { get; set; } = null!;
    public List<RoyalePublisherGameJson> PublisherGames { get; set; } = [];
    public decimal Budget { get; set; }
}

internal sealed class RoyalePublisherGameJson
{
    public MasterGameYearJson? MasterGame { get; set; }
    public bool GameHidden { get; set; }
    public decimal AdvertisingMoney { get; set; }
    public decimal? AmountSpent { get; set; }
}

internal sealed class MasterGameYearJson
{
    public Guid MasterGameID { get; set; }
}

internal sealed class PossibleRoyaleMasterGameJson
{
    public MasterGameYearJson MasterGame { get; set; } = null!;
    public bool IsAvailable { get; set; }
    public decimal Cost { get; set; }
    public string Status { get; set; } = "";
}

internal sealed class PlayerClaimResultJson
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = [];
}
