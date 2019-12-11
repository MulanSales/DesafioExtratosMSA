using System;
using CommonLibrary.DataAccess.Abstractions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Statements.API.Models {

    public class Release : ICollectionSchema
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Date { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public string EstablishmentName { get; set; }

        public Decimal Amount { get; set; }
        
        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }

}
