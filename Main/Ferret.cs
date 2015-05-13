using AnonymousFerretTwitchLogger.Config;
using AnonymousFerretTwitchLogger.Console;
using AnonymousFerretTwitchLogger.IRC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AnonymousFerretTwitchLogger.Main
{
    public class Ferret
    {

        private ConsoleListener console;
        private bool isRunning = false;
        private Client ircClient;

        public static Ferret Instance { get; protected set; }
        public static ConfigManager Settings { get; protected set; }

        public Ferret(string[] args)
        {
            this.console = new ConsoleListener();
            Instance = this;
            Settings = new ConfigManager();
            this.ircClient = new Client("irc.twitch.tv", 6667);
        }


        public static void Main(string[] args)
        {
            Ferret f = new Ferret(args);
            f.startThreads();
        }

        public void startThreads()
        {
            /*this.thread = new Thread(new ThreadStart(this.run)); // Stop the bot auto closing when the IRC thread dies for whatever reason.
            this.isRunning = true;
            this.thread.Start();*/
            this.console.start();
            if (Settings.get<bool>("AutoConnectonStart"))
                this.ircClient.connect();
        }

        public void run()
        {
            while (this.isRunning)
            {
                Thread.Sleep(1000);
            }

        }

    }
}
