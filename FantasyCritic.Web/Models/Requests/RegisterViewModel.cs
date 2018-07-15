namespace FantasyCritic.Web.Models.Requests
{
    public class RegisterViewModel
    {
        public string EmailAddress { get; set; }
        public string UserName { get; set; }
        public string RealName { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
