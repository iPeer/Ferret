using AnonymousFerretTwitchLogger.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnonymousFerretTwitchLogger.Config
{
    public class ConfigManager
    {

        private string configDirectory = "./config/";
        private string configPath = "./config/config.cfg";
        private Dictionary<string, object> SETTINGS_MAP = new Dictionary<string, object>{

            {"AutoConnectonStart", true},
            {"LogChatToConsole", false},
            {"AutoJoinChannels", "#theipeer,#ksptv"}, // Separate with commas!
            {"ServerAddress", "irc.twitch.tv"},
            {"ServerPort", "6667"},
            {"TwitchUsername", "AnonymousFerret"}, // Change this in your config!
            {"UseV3API", true}

        };

        public ConfigManager()
        {
            if (!Directory.Exists(this.configDirectory))
                Directory.CreateDirectory(configDirectory);
        }

        public void load()
        {

        }

        public T get<T>(string setting)
        {
            if (this.SETTINGS_MAP.ContainsKey(setting))
            {
                if (this.SETTINGS_MAP[setting] is T)
                    return (T)this.SETTINGS_MAP[setting];
                else
                {
                    try
                    {
                        return (T)Convert.ChangeType(this.SETTINGS_MAP[setting], typeof(T));
                    }
                    catch (InvalidCastException)
                    {
                        return default(T);
                    }
                }
            }
            Logger.Log("Attempted to read invalid setting '{0}'!", Logger.Level.LOG, setting);
            return default(T);
        }

    }
}
