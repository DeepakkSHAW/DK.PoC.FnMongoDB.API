using DnsClient.Protocol;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DK.PoC.FnMongoDB.API.MongoCollectionSettings
{
    public class ContactSettings
    {
        public MongoClientSettings mongoDBClientSetting { get; set; }
        public MongoClient mongoClient { get; set; }
        public string MdbConnectionstring { get; set; }
        public string DatabaseName { get; set; }
        public string ContactCollection { get; set; }
    }
}
