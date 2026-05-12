using System.Text;
using System.Xml.Linq;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Services;
using FantasyCritic.Web.Models.Responses;
using FantasyCritic.Web.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace FantasyCritic.Web.Controllers.API;

[Route("api/[controller]/[action]")]
public class GeneralController : FantasyCriticController
{
    private readonly InterLeagueService _interLeagueService;
    private readonly IClock _clock;


    public GeneralController(InterLeagueService interLeagueService, FantasyCriticUserManager userManager, IClock clock) : base(userManager)
    {
        _interLeagueService = interLeagueService;
        _clock = clock;
    }

    public async Task<ActionResult<SiteCountsViewModel>> SiteCounts()
    {
        var counts = await _interLeagueService.GetSiteCounts();
        return Ok(new SiteCountsViewModel(counts));
    }

    public async Task<ActionResult<List<string>>> Donors()
    {
        var donors = await _userManager.GetDonors();
        return Ok(donors);
    }

    public async Task<ActionResult<BidTimesViewModel>> BidTimes()
    {
        var systemWideSettings = await _interLeagueService.GetSystemWideSettings();
        var vm = DomainControllerUtilities.BuildBidTimesViewModel(_clock, systemWideSettings);
        return Ok(vm);
    }

    public async Task<ActionResult<List<SiteAnnouncementViewModel>>> SiteAnnouncements()
    {
        var announcements = await _interLeagueService.GetSiteAnnouncements();
        var vms = announcements.Select(x => new SiteAnnouncementViewModel(x)).ToList();
        return Ok(vms);
    }

    [HttpGet]
    [HttpGet("/rss/announcements")]
    public async Task<IActionResult> SiteAnnouncementsRss()
    {
        var announcements = await _interLeagueService.GetSiteAnnouncements();
        XNamespace contentNs = "http://purl.org/rss/1.0/modules/content/";

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var siteUpdatesUrl = $"{baseUrl}/siteUpdates";

        var items = announcements.Select(a =>
        {
            var pubDate = new DateTimeOffset(a.PostedAt.ToDateTimeUtc()).ToString("R");
            var descriptionText = a.Body;
            var item = new XElement("item",
                new XElement("title", a.Title),
                new XElement("description", new XCData(descriptionText)),
                new XElement(contentNs + "encoded", new XCData(descriptionText)),
                new XElement("pubDate", pubDate),
                new XElement("guid", new XAttribute("isPermaLink", "false"), a.Id.ToString())
            );
            if (!string.IsNullOrEmpty(a.LinkAddress))
            {
                item.Add(new XElement("link", a.LinkAddress));
            }
            return item;
        });

        var feed = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement("rss",
                new XAttribute("version", "2.0"),
                new XAttribute(XNamespace.Xmlns + "content", contentNs),
                new XElement("channel",
                    new XElement("title", "Fantasy Critic Announcements"),
                    new XElement("link", siteUpdatesUrl),
                    new XElement("description", "Site announcements from Fantasy Critic"),
                    items
                )
            )
        );

        var xml = feed.Declaration + feed.ToString();
        return Content(xml, "application/rss+xml", Encoding.UTF8);
    }
}
