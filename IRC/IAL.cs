using AnonymousFerretTwitchLogger.Logging;
using AnonymousFerretTwitchLogger.Main;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnonymousFerretTwitchLogger.IRC
{

    public class Channel
    {
        public string ChannelName { get; private set; }
        public Dictionary<string, User> userList = new Dictionary<string, User>();
        public List<Ban> banList = new List<Ban>();
        public string HostTarget { get; set; }


        public Channel(string name)
        {
            ChannelName = name.ToLower();
            HostTarget = String.Empty;
        }
    }

    public class User
    {
        public string Username { get; private set; }
        public List<Message> messageHistory = new List<Message>();

        public User(string name)
        {
            Username = name.ToLower();
        }
    }

    public class Message
    {
        public string MessageText { get; protected set; }
        public DateTime Timestamp { get; protected set; }

        public Message(string message, DateTime stamp)
        {
            MessageText = message;
            Timestamp = stamp;
        }
    }

    public class Ban
    {
        public string Username { get; private set; }
        public DateTime When { get; private set; }

        public Ban(string user, DateTime stamp)
        {
            Username = user;
            When = stamp;
        }

    }

    public class IAL
    {

        public static Dictionary<string, Channel> channels = new Dictionary<string, Channel>();
        public static IAL Instance { get; protected set; }

        public IAL()
        {
            Instance = this;
        }

        public static User getUserFromChannel(string channel, string user)
        {

            string channelLower = channel.ToLower();
            if (!channelLower.StartsWith("#"))
                channelLower = "#" + channelLower;
            string nickLower = user.ToLower();

            if (channels.ContainsKey(channelLower))
            {
                Channel c = channels[channelLower];
                if (c.userList.ContainsKey(nickLower))
                    return c.userList[nickLower];
                else
                    throw new NoSuchNickException("Nick is not present in that channel");
            }
            else
                throw new NoSuchChannelException("Not currently in that channel!");
                

        }

        public static bool isChannelRegistered(string channel)
        {
            return channels.ContainsKey(channel.ToLower());
        }

        public static Channel RegisterChannel(string channel)
        {
            string lowerChannel = channel.ToLower();
            if (!channels.ContainsKey(lowerChannel))
            {
                Channel c = new Channel(lowerChannel);
                channels.Add(lowerChannel, c);
                Logger.Log("Registered channel '{0}'", Logger.Level.LOG, channel);
                return c;
            }
            else
                return channels[lowerChannel];
        }

        public static User RegisterNick(string channel, string nick)
        {
            string channelLower = channel.ToLower();
            string nickLower = nick.ToLower();
            Channel c;
            if (isChannelRegistered(channelLower))
                c = channels[channelLower];
            else
                c = RegisterChannel(channelLower);
            if (c.userList.ContainsKey(nickLower))
                return c.userList[nickLower];
            else
            {
                User u = new User(nickLower);
                c.userList.Add(nickLower, u);
                return u;
            }

        }

        public static void addNickMessage(string channel, string nick, string message)
        {
            string channelLower = channel.ToLower();
            string nickLower = nick.ToLower();
#if DEBUG

            //Debug.WriteLine(channel + " / " + nick + " / " + message);

#endif
            User n;
            try
            {
                n = getUserFromChannel(channel, nick);
            }
            catch (NoSuchNickException)
            {
                n = RegisterNick(channel, nick);
            }
            n.messageHistory.Add(new Message(message, DateTime.Now));
        }

        public static void RegisterBan(string channel, string nick)
        {
            string channelLower = channel.ToLower();
            string nickLower = nick.ToLower();
            if (channels.ContainsKey(channelLower))
            {
                Channel c = channels[channelLower];
                c.banList.Add(new Ban(nickLower, DateTime.Now));
            }
        }

        public static void UnregisterUser(string channel, string nick)
        {
            string channelLower = channel.ToLower();
            string nickLower = nick.ToLower();
            if (channels.ContainsKey(channelLower))
            {

                Channel c = channels[channelLower];
                if (c.userList.ContainsKey(nickLower))
                {
                    Logger.Log("Unregistered nick '{0}' from channel '{1}'", Logger.Level.LOG, nick, channel);
                    c.userList.Remove(nickLower);
                }
            }
        }

        public static void UnregisterChannel(string channel)
        {
            string channelLower = channel.ToLower();
            if (channels.ContainsKey(channelLower))
            {
                Logger.Log("unregistered channel '{0}'", Logger.Level.LOG, channel);
                channels.Remove(channelLower);
            }
        }


        public static Channel getChannel(string channel)
        {
            if (!channel.StartsWith("#"))
                channel = "#" + channel;
            if (channels.ContainsKey(channel.ToLower()))
                return channels[channel.ToLower()];
            else
                throw new NoSuchChannelException();
        }
    }
}
