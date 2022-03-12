namespace FantasyCritic.MySQL.Entities
{
    public class RoyaleStandingsEntity
    {
        public Guid PublisherID { get; set; }
        public int Year { get; set; }
        public int Quarter { get; set; }
        public decimal TotalFantasyPoints { get; set; }
    }
}
