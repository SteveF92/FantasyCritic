namespace FantasyCritic.Web.Models.Requests
{
    public class RegisterRequest
    {
        public string EmailAddress { get; set; }
        public string DisplayName { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
