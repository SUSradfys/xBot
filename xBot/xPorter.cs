using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EvilDICOM.Network;
using System.Xml;
using System.Data;
using System.Net;

namespace xBot
{
    class xPorter
    {
        private Entity reciever;
        private DICOMSCP scp;
        private string ip, AEtitle, sqlString, lastActive, name;
        private int port;
        List<string> items;
        bool active, doublets;

        public xPorter(XmlNode node)
        {
            this.ip = node.SelectSingleNode("//ipstring").InnerText;
            this.AEtitle = node.SelectSingleNode("//AEtitle").InnerText;
            Int32.TryParse(node.SelectSingleNode("//port").InnerText, out this.port);
            this.reciever = new Entity(AEtitle, getIP(ip), port);
            this.scp = new DICOMSCP(reciever);
            this.active = XmlConvert.ToBoolean(node.SelectSingleNode("//active").InnerText);
            this.lastActive = node.SelectSingleNode("//lastActivity").InnerText;
            this.sqlString = node.SelectSingleNode("//SQLstring").InnerText;
            this.name = node.SelectSingleNode("//name").InnerText;
            this.items = new List<string>();
            XmlNodeList nodeItems = node.SelectNodes("//include/item");
            foreach(XmlNode nodeItem in nodeItems)
            {
                this.items.Add(nodeItem.InnerText);
            }
            this.doublets = XmlConvert.ToBoolean(node.SelectSingleNode("//allowDoublets").InnerText);
        }

        public DICOMSCP Scp
        {
            get { return this.scp; }
        }

        public string SqlString
        {
            get { return this.sqlString; }
        }

        public string Name
        {
            get { return this.name; }
        }

        public List<string> Items
        {
            get { return this.items; }
        }

        public bool Active
        {
            get { return this.active; }
        }

        public bool Doublets
        {
            get { return this.doublets; }
        }

        public DataTable Query()
        {
            DataTable plans = SqlInterface.Query(this.sqlString.Replace("this.lastActive", this.lastActive));
            return plans;
        }

        private string getIP(string dns)
        {
            string ip = String.Empty;
            try
            {
                ip = Dns.GetHostAddresses(dns)[0].ToString();
            }
            catch
            {
                ip = dns;
            }
            return ip;
        }
    }
}
