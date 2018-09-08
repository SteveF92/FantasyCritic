namespace FantasyCritic.Web.Models.Requests
{
    public class TokenRefreshRequest
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
}
