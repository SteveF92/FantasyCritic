namespace FantasyCritic.Lib.Domain.Results
{
    public class ClaimError
    {
        public ClaimError(string error, bool overridable, bool noSpaceError = false)
        {
            Error = error;
            Overridable = overridable;
            NoSpaceError = noSpaceError;
        }

        public string Error { get; }
        public bool Overridable { get; }
        public bool NoSpaceError { get; }
    }
}
