using OpenMetaverse;

namespace Multibot
{
    public class LocalAvatar 
    {
        public required Simulator Sim;
        public required Avatar Agent;
    }

    class Multibot
    {
        private GridClient client;
        private LoginParams loginParams;
        private List<string> admins;

        public Multibot(BotCreds credentials, List<string> admins)
        {
            client = new GridClient();
            client.Settings.MULTIPLE_SIMS = true;

            loginParams = new LoginParams(client, (LoginCredential)credentials, "", "")
            {
                AgreeToTos = true
            };
            if (credentials.LoginUrl != null) { loginParams.URI = credentials.LoginUrl; }
            if (credentials.LoginLocation != null) { loginParams.LoginLocation = credentials.LoginLocation; }

            this.admins = admins;

            client.Self.IM += OnIm;
            client.Self.ChatFromSimulator += OnChatFromSimulator;

        }

        private void OnChatFromSimulator(object? sender, ChatEventArgs args)
        {
            // Ignore chat from non-admins
            if (!admins.Contains(args.OwnerID.ToString()))
            {
                Logger.Log($"Ignoring chat from {args.OwnerID}", Helpers.LogLevel.Debug);
                return;
            }

            if (args.Message.StartsWith("!"))
            {
                ParseCommand(args.Message[1..], args.OwnerID);
            }
        }

        private void OnIm(object? sender, InstantMessageEventArgs args)
        {
            // Ignore IMs from non-admins
            if (!admins.Contains(args.IM.FromAgentID.ToString()))
            {
                Logger.Log($"Ignoring chat from {args.IM.FromAgentID}", Helpers.LogLevel.Debug);
                return;
            }

            switch (args.IM.Dialog)
            {
                case InstantMessageDialog.RequestTeleport:
                    client.Self.TeleportLureRespond(args.IM.FromAgentID, args.IM.IMSessionID, true);
                    break;
                case InstantMessageDialog.MessageFromAgent:
                    if (args.IM.Message.StartsWith("!"))
                    {
                        ParseCommand(args.IM.Message[1..], args.IM.FromAgentID);
                    }
                    break;

            }
        }

        public void ParseCommand(string cmd, UUID agentId)
        {
            switch (cmd)
            {
                case "come":
                    var a = FindAvatarById(agentId);
                    if (a == null)
                    {
                        Logger.Log($"Unable to find agent {agentId} in current or neighboring regions.", Helpers.LogLevel.Warning);
                        return;
                    }
                    Utils.LongToUInts(a.Sim.Handle, out var x, out var y);
                    client.Self.AutoPilot(a.Agent.Position.X + x, a.Agent.Position.Y + y, a.Agent.Position.Z);
                    break;
                default:
                    Logger.Log($"Unknown command {cmd}", Helpers.LogLevel.Info);
                    break;
            }
        }

        private LocalAvatar? FindAvatarById(UUID id)
        {
            foreach (var sim in client.Network.Simulators)
            {
                var av = sim.ObjectsAvatars.Find(a => a.ID == id);
                if (av != null)
                {
                    return new LocalAvatar { Sim = sim, Agent = av };
                }
            }
            return null;
        }

        public Task Start()
        {
            var tsc = new TaskCompletionSource<bool>();

            if (client.Network.Connected)
            {
                tsc.SetResult(true);
                return tsc.Task;
            }

            EventHandler<LoginProgressEventArgs>? onLoginProgress = null;
            client.Network.LoginProgress += onLoginProgress = (sender, args) => {
                switch (args.Status) {
                    case LoginStatus.Failed:
                        tsc.SetResult(true);
                        break;
                    case LoginStatus.Success:
                        tsc.SetResult(false);
                        break;
                }
                client.Network.LoginProgress -= onLoginProgress;
            };

            client.Network.BeginLogin(loginParams);

            return tsc.Task;
        }

        public Task Stop()
        {
            var tsc = new TaskCompletionSource();

            if (!client.Network.Connected)
            {
                tsc.SetResult();
                return tsc.Task;
            }

            EventHandler<LoggedOutEventArgs>? onLogout = null;
            client.Network.LoggedOut += onLogout = (sender, args) => {
                tsc.SetResult();
                client.Network.LoggedOut -= onLogout;
            };

            client.Network.BeginLogout();

            return tsc.Task;
        }
    }
}
