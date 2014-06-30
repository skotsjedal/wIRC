using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace wIRC.Config
{
    public static class Conf
    {
        public static IrcConfig IrcConfig
        {
            get { return ConfigurationManager.GetSection("ircconfig") as IrcConfig; }
        }
    }

    public class IrcConfig : ConfigurationSection
    {
        [ConfigurationProperty("defaultNick", IsRequired = true)]
        public string DefaultNick
        {
            get { return base["defaultNick"] as string; }
        }

        [ConfigurationProperty("servers")]
        public Servers Servers
        {
            get { return base["servers"] as Servers; }
        }
    }

    [ConfigurationCollection(typeof (Server), AddItemName = "server",
        CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class Servers : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        protected override string ElementName
        {
            get { return "server"; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new Server();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as Server).Name;
        }

        public Server this[int index]
        {
            get { return (Server) BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                base.BaseAdd(index, value);
            }
        }

        public new Server this[string name]
        {
            get { return BaseGet(name) as Server; }
        }
    }

    public class Server : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return base["name"] as string; }
        }

        [ConfigurationProperty("endpoint", IsRequired = true)]
        public string Endpoint
        {
            get { return base["endpoint"] as string; }
        }

        [ConfigurationProperty("port", IsRequired = true)]
        public int Port
        {
            get { return base["port"] is int ? (int) base["port"] : 0; }
        }

        [ConfigurationProperty("nick", IsRequired = false)]
        public string Nick
        {
            get { return base["nick"] as string; }
        }

        [ConfigurationProperty("channels", IsRequired = false)]
        public string Channels
        {
            get { return base["channels"] as string; }
        }

        public List<string> ChannelsList
        {
            get { return Channels != null ? Channels.Split(new char[';']).ToList() : new List<string>(); }
        }
    }
}