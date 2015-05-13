using AnonymousFerretTwitchLogger.Main;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnonymousFerretTwitchLogger.Logging
{
    public class Logger
    {

        private static object locker = new Object();
        private static object channel_locker = new object();

        public enum Level
        {
            ALL = 0,
            CONSOLE = 1,
            CONSOLE_AND_LOG = 2,
            LOG = 3, // Everything is logged to the log by default, this should be here, really.
            NONE = 4
        }

        public static void Log(string s, params string[] format)
        {
            Log(s, Level.CONSOLE_AND_LOG, format);
        }

        public static void logGenericChannelMessageWithTimestamp(string channel, string message)
        {
            logGenericChannelMessage(channel, "[" + DateTime.Now.ToString("U") + "] " + message);
        }

        public static void logGenericChannelMessage(string channel, string message)
        {

            string channelLogPath = "./logs/channels/";
            if (!Directory.Exists(channelLogPath))
                Directory.CreateDirectory(channelLogPath);
            string chanName = channel.Replace("#", "");
            string logName = channelLogPath + chanName + ".log";
            try
            {

                lock (channel_locker)
                {
                    using (FileStream fs = new FileStream(logName, FileMode.Append, FileAccess.Write, FileShare.Read))
                    using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                    {
                        sw.Write(message+(message.EndsWith("\n") ? "" : "\n"));
                    }
                }
                //File.WriteAllLines(String.Format("../logs/channels/{0}.log", chanName), new string[] { String.Format("[{0}] {1}: {2}", DateTime.Now.ToString("U"), user, message) }, Encoding.Unicode);
            }
            catch (Exception e)
            {
                Log("Unable to save to log file! {1}\n{0}", Level.CONSOLE, e.StackTrace, e.ToString());
            }

        }

        public static void LogChatHost(string channel, string hostTarget)
        {
            if (hostTarget.Equals("-"))
                logGenericChannelMessageWithTimestamp(channel, "[HOST] Exited host mode.");
            else
                logGenericChannelMessageWithTimestamp(channel, "[HOST] Now hosting " + hostTarget);
        }

        public static void LogChatMessage(string channel, string user, string message)
        {
            if (Ferret.Settings.get<bool>("LogChatToConsole"))
                Log("[{0}] [{3}] {1}: {2}", Level.CONSOLE, DateTime.Now.ToString("U"), user, message, channel);
            logGenericChannelMessage(channel, String.Format("[{0}] {1}: {2}\n", DateTime.Now.ToString("U"), user, message));
        }

        public static void LogChatJoin(string channel, string nick)
        {
            logGenericChannelMessageWithTimestamp(channel, nick + " joined.");
        }

        public static void LogChatPart(string channel, string nick)
        {
            logGenericChannelMessageWithTimestamp(channel, nick + " parted.");
        }

        public static void Log(string s, Level level, params string[] format) 
        {
            if (level == Level.NONE)
                return;
            string realString = String.Format(s, format);
            if (level == Level.CONSOLE || level == Level.CONSOLE_AND_LOG || level == Level.ALL)
                System.Console.WriteLine(realString);
                string path = "./logs/";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                try
                {
                    /*TextWriter w = TextWriter.Synchronized(File.AppendText(path + "bot.log"));
                    w.WriteLine("[{0}] {1}", DateTime.Now.ToString("U"), realString);
                    //File.WriteAllLines(path + "bot.log", new string[] { String.Format("[{0}] {1}", DateTime.Now.ToString("U"), realString) }, Encoding.Unicode);*/
                    lock (locker)
                    {
                        using (FileStream fs = new FileStream(path + "bot.log", FileMode.Append, FileAccess.Write, FileShare.Read))
                        using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                        {
                            sw.Write("[{0}] {1}\n", DateTime.Now.ToString("U"), realString);
                        }
                    }


                }
                catch (Exception e)
                {
                    Log("Unable to save to log file! {1}\n{0}", Level.CONSOLE, e.StackTrace, e.ToString());
                }
        }

    }
}
