using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DK.PoC.FnMongoDB.API.Models
{
    public interface IDemoApp
    {
        void SayHi();
    }

    public class DemoApp : IDemoApp
    {
        public void SayHi()
        {
            Console.WriteLine("Hi there..");
        }
    }

    public class Contact
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("surname")]
        public string LastName { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }

        [BsonElement("message")]
        public string Message { get; set; }

        //[BsonRepresentation(BsonType.Timestamp)]
        //[BsonElement("inDate")]
        //public Timestamp InDate { get; set; }
    }
}
