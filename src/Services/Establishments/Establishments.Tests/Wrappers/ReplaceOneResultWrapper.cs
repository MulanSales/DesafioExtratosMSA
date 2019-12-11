using MongoDB.Bson;
using MongoDB.Driver;

namespace Establishments.Tests.Wrappers
{
    public class ReplaceOneResultWrapper : ReplaceOneResult
    {
        public ReplaceOneResultWrapper() {}
        public override bool IsAcknowledged => false;

        public override bool IsModifiedCountAvailable => true; 

        public override long MatchedCount => 0;

        public override long ModifiedCount => 0;

        public override BsonValue UpsertedId => new BsonInt32(1);
    }
}