namespace FantasyCritic.Web.Models.Responses
{
    public class PublisherSlotRequirementsViewModel
    {
        public PublisherSlotRequirementsViewModel(LeagueOptions options)
        {
            RegularSlot = new PublisherSingleSlotRequirementsViewModel(options.LeagueTags);
            if (!options.HasSpecialSlots())
            {
                OverallSlot = RegularSlot;
                SpecialSlots = new List<PublisherSingleSlotRequirementsViewModel>();
            }
            else
            {
                var distinctSpecialSlots = options.SpecialGameSlots.DistinctBy(x => string.Join(",", x.Tags.OrderBy(x => x.Name))).ToList();
                var specialSlotRequiredTags = distinctSpecialSlots.SelectMany(x => x.Tags.Select(y => y.Name)).Distinct().ToList();
                var regularBannedTags = options.LeagueTags.Where(x => x.Status.Equals(TagStatus.Banned)).Select(x => x.Tag.Name).ToList();
                var overallBannedTags = regularBannedTags.Except(specialSlotRequiredTags).ToList();
                OverallSlot = new PublisherSingleSlotRequirementsViewModel(overallBannedTags);

                var specialSlots = new List<PublisherSingleSlotRequirementsViewModel>();
                foreach (var specialSlot in distinctSpecialSlots)
                {
                    var specialSlotTags = options.LeagueTags.ToList();
                    specialSlotTags.AddRange(specialSlot.Tags.Select(x => new LeagueTagStatus(x, TagStatus.Required)));
                    specialSlots.Add(new PublisherSingleSlotRequirementsViewModel(specialSlotTags));
                }

                SpecialSlots = specialSlots;
            }

            if (options.CounterPicks > 0)
            {
                CounterPickSlot = new PublisherSingleSlotRequirementsViewModel(true);
            }
        }

        public PublisherSingleSlotRequirementsViewModel OverallSlot { get; }
        public PublisherSingleSlotRequirementsViewModel RegularSlot { get; }
        public IReadOnlyList<PublisherSingleSlotRequirementsViewModel> SpecialSlots { get; }
        public PublisherSingleSlotRequirementsViewModel CounterPickSlot { get; }

    }
}
