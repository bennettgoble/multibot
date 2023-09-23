using OpenMetaverse;

namespace Multibot 
{
    public class Config
    {
        public List<string>? Admins { get; set; }
        public required List<BotCreds> Bots { get; set; }
    }

    public class BotCreds
    {
        public required string First { get; set; }
        public required string Last { get; set; }
        public required string Pass { get; set; }

        public static explicit operator LoginCredential(BotCreds creds)
        {
            return new LoginCredential(creds.First, creds.Last, creds.Pass);
        }
    }
}