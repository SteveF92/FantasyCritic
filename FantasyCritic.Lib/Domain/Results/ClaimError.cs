namespace FantasyCritic.Lib.Domain.Results
{
    public class ClaimError
    {
        public ClaimError(string error, bool overridable)
        {
            Error = error;
            Overridable = overridable;
        }

        public string Error { get; }
        public bool Overridable { get; }
    }
}
