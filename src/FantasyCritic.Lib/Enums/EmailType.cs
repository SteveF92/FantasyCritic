namespace FantasyCritic.Lib.Enums
{
    public class EmailType : TypeSafeEnum<EmailType>
    {

        // Define values here.
        public static readonly EmailType PublicBids = new EmailType("PublicBids", "Public Bids");

        // Constructor is private: values are defined within this class only!
        private EmailType(string value, string readableName)
            : base(value)
        {
            ReadableName = readableName;
        }

        public string ReadableName { get; }


        public override string ToString() => Value;
    }
}
