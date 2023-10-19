﻿using CommandLine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Multibot 
{
    public class Options
    {
        [Option('c', "config", Required = true, HelpText = "Path to YAML config file.")]
        public required string ConfigFile { get; set; }

        [Option('u', "login-url", Required = false, HelpText = "Login URL.")]
        public string? LoginUrl { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsedAsync(o => {
                    using var reader = new StreamReader(o.ConfigFile);
                    var deserializer = new DeserializerBuilder()
                        .WithNamingConvention(LowerCaseNamingConvention.Instance)
                        .Build();

                    // Load config
                    var cfg = deserializer.Deserialize<Config>(reader);
                    var admins = cfg.Admins ?? new List<string>();

                    // Initialize and start bots
                    var bots = new List<Multibot>();
                    foreach (var creds in cfg.Bots)
                    {
                        var bot = new Multibot(creds, admins);
                        bot.Start();
                        bots.Add(bot);
                    }

                    Console.CancelKeyPress += async delegate
                    {
                        // Wait for all bots to disconnect
                        var stopTasks = bots.Select(b => b.Stop()).ToArray();
                        await Task.WhenAll(stopTasks);
                    };

                    // Run until cancelled
                    while (true) { }
                });
        }
    }
}
