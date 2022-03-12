namespace FantasyCritic.MySQL.Entities
{
    internal class TagOverrideEntity
    {
        public Guid LeagueID { get; set; }
        public int Year { get; set; }
        public Guid MasterGameID { get; set; }
        public string TagName { get; set; }
    }
}
