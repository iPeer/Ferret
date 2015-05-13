using AnonymousFerretTwitchLogger.IRC;
using AnonymousFerretTwitchLogger.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AnonymousFerretTwitchLogger.Console
{
    public class ConsoleListener
    {

        private Thread thread;
        private bool isRunning = false;

        public void start()
        {
            this.thread = new Thread(new ThreadStart(this.run));
            this.thread.Start();
            this.isRunning = true;
        }

        public void run()
        {
            while (this.isRunning)
            {
                string[] commandData = System.Console.ReadLine().Split(' ');
                string command = commandData[0];
                if (commandData.Length >= 2)
                {
                    string commandParam = commandData[1];
                    string[] cParams = new string[commandData.Length - 2];
                    for (int x = 2; x < commandData.Length; x++)
                        cParams[x - 2] = commandData[x];

                    if (command.Equals("chat"))
                    {
                        if (commandParam.Equals("history"))
                        {
                            string channel = cParams[0];
                            string nick = cParams[1];
                            int limit = 5;
                            if (cParams.Length >= 3)
                                limit = Convert.ToInt32(cParams[2]);

                            User n = IAL.getUserFromChannel(channel, nick);

                            List<Message> history = n.messageHistory;
                            history.Sort(delegate(Message a, Message b)
                            {
                                return DateTime.Compare(b.Timestamp, a.Timestamp);
                            });

                            Logger.Log("Showing {0} most recent messages for user '{1}'", Logger.Level.CONSOLE, (history.Count < limit ? history.Count : limit).ToString(), nick);

                            for (int x = 0; x < limit && x < history.Count; x++)
                            {
                                Logger.Log("\t{0}: {1}", Logger.Level.CONSOLE, (x + 1).ToString(), history[x].MessageText);
                            }



                        }
                    }
                    else if (command.Equals("console"))
                    {
                        if (commandParam.Equals("clear"))
                            System.Console.Clear();
                    }
                    else if (command.Equals("stats"))
                    {

                        if (commandParam.Equals("bans"))
                        {
                            string channel = cParams[0].ToLower();
                            //Debug.WriteLine(channel);
                            try
                            {
                                int seconds = 600;
                                if (cParams.Length >= 2)
                                    seconds = Convert.ToInt32(cParams[1]);
                                DateTime limit = new DateTime();
                                limit.AddSeconds(seconds);
                                DateTime now = DateTime.Now;
                                now.Subtract(limit);

                                Channel c = IAL.getChannel(channel);
                                System.Console.WriteLine("{0} bans in the last {1} seconds", c.banList.Count(b => b.When.CompareTo(now) >= 0), seconds);
                            }
                            catch (NoSuchChannelException) { System.Console.WriteLine("Not in that channel!"); }
                            
                        }
                           

                    }
                    else
                        System.Console.WriteLine("Invalid command!");

                }
            }
        }

    }
}
