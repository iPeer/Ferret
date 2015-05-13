using AnonymousFerretTwitchLogger.Logging;
using AnonymousFerretTwitchLogger.Main;
using AnonymousFerretTwitchLogger.IRC;
using AnonymousFerretTwitchLogger.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace AnonymousFerretTwitchLogger.IRC
{
    public class Client
    {

        public static Client Instance { get; protected set; }
        public string server { get; set; }
        public int port { get; set; }
        public Status currentStatus = Status.NOT_CONNECTED;

        public Socket socket;
        private TcpClient tcpClient;
        public bool isRunning = false;

        private StreamReader read;
        private StreamWriter write;
        private string MY_USERNAME = "";

        protected Thread thread;

        public enum Status
        {
            CONNECTING = 0,
            NOT_CONNECTED = 1,
            CONNECTED = 2,
            DISCONNECTED = 3,
        }

        public Client(string server, int port)
        {
            this.server = server;
            this.port = port;
            Instance = this;
        }

        public void connect()
        {
            Logger.Log("Connecting to IRC...", Logger.Level.LOG);
            Thread _thread = new Thread(new ThreadStart(this.run));
            this.currentStatus = Status.CONNECTING;
            this.isRunning = true;
            _thread.Start();
            this.thread = _thread;
        }

        public void run()
        {
            this.tcpClient = new TcpClient(server, port);
            this.read = new StreamReader(this.tcpClient.GetStream()/*, Encoding.UTF8*/);
            this.write = new StreamWriter(this.tcpClient.GetStream()/*, Encoding.UTF8*/);

            string s = String.Empty;
            String nick = Ferret.Settings.get<string>("TwitchUsername");
            Logger.Log("Sending nick '{0}'", Logger.Level.LOG, nick);
            this.MY_USERNAME = nick;
            onConnect();
            while ((s = this.read.ReadLine()) != null && (this.currentStatus == Status.CONNECTING || this.currentStatus == Status.CONNECTED) && isRunning)
            {
                parseLine(s);
            }


        }

        public void parseLine(string line)
        {
            Logger.Log("<- " + line, Logger.Level.LOG);
            string[] spacedInput = line.Split(' ');
            string command = spacedInput[1];
            string nick = spacedInput[0].Substring(1).Split('!')[0];
            int indexOfCommand = line.IndexOf(command) + command.Length + 1;
            string[] parameters = new string[0];
            try
            {
                parameters = line.Substring(indexOfCommand).Split(' ');
            }
            catch (ArgumentOutOfRangeException) { }
            //Logger.Log("DEBUG: PARAMETERS = {0}", Logger.Level.CONSOLE, parameters.Join(", "));
            /*else if (command.Equals("004")) // We're connected
                onConnect();*/

            if (command.Equals("PING")) // PING/PONG event
                send("PONG " + line.Substring(5));
            else if (command.Equals("353")) // Nicklist on channel join
                onNickListOnJoin(spacedInput[4], line.Substring(1).Split(':')[1].Split(' ')); // [12 May 2015 20:24:10] <- :anonymousferret.tmi.twitch.tv 353 anonymousferret = #theipeer :anonymousferret
            else if (command.Equals("JOIN"))
            {
                string channel = spacedInput[2];
                if (nick.Equals(Ferret.Settings.get<string>("TwitchUsername")))
                    Logger.Log("JOINED: " + channel);
                else
                    Logger.LogChatJoin(channel, nick);
            }
            else if (command.Equals("PRIVMSG"))
            {
                string channel = parameters[0];
                string message = line.Substring(line.IndexOf(':', 2) + 1);
                if (channel.Equals(this.MY_USERNAME))
                    channel = nick;
                if (nick == "jtv")
                {

                    string[] jtvParams = message.Split(' ');
                    if (jtvParams[0].Equals("HOSTTARGET"))
                        Logger.LogChatHost(channel, jtvParams[1]);
                    if (jtvParams[0].Equals("CLEARCHAT") && jtvParams.Length == 2)
                    {
                        try
                        {
                            IAL.RegisterBan(channel, jtvParams[2]);
                        }
                        catch (NoSuchChannelException) { }
                    }
                    else if (jtvParams[0].Equals("CLEARCHAT") && jtvParams.Length == 1)
                    {
                        Logger.logGenericChannelMessageWithTimestamp(channel, "[CHATCLEAR] Chat was cleared by a moderator");
                    }
#if DEBUG
                    //Debug.WriteLine("JTV: "+message);
#endif
                    return;

                }
                else
                    IAL.addNickMessage(channel, nick, message);
                Logger.LogChatMessage(channel, nick, message);
            }
              

        }

        public void onConnect()
        {
            this.currentStatus = Status.CONNECTED;
            send("PASS {0}", <...>); // TODO: Read this from an (encrypted) file
            send("NICK " + Ferret.Settings.get<string>("TwitchUsername"));
            if (Ferret.Settings.get<bool>("UseV3API"))
                send("TWITCHCLIENT 3");
            else
                send("TWITCHCLIENT");
            string autojoin = Ferret.Settings.get<string>("AutoJoinChannels");
            if (autojoin != string.Empty && !autojoin.Equals(""))
                foreach (string c in autojoin.Split(','))
                    joinChannel(c.ToLower());
        }

        public void onNickListOnJoin(string channel, string[] nicks)
        {
            foreach (string n in nicks)
                IAL.RegisterNick(channel, n);
        }

        public void send(string text, params string[] formats) 
        {
            string _text = String.Format(text, formats);
            //if (!(_text.StartsWith("PONG ") && _text.StartsWith("PASS ")))
                Logger.Log("-> " + _text, Logger.Level.LOG);
            this.write.WriteLine(_text);
            this.write.Flush();
        }

        public void joinChannel(string channel)
        {
            send(String.Format("JOIN {0}", (channel.StartsWith("#") ? "" : "#") + channel));
            IAL.RegisterChannel(channel);
       }


    }
}
