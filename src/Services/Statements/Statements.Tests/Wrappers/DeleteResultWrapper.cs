using MongoDB.Driver;

namespace Statements.Tests.Wrappers
{
    public class DeleteResultWrapper : DeleteResult 
    {
        public DeleteResultWrapper() {}

        public override long DeletedCount => 1;

        public override bool IsAcknowledged => false;
    }
}