using System.Threading.Tasks;
using CommonLibrary.DataAccess;
using CommonLibrary.DataAccess.Abstractions;
using MongoDB.Driver;
using Releases.API.Models;

namespace Releases.API.Services
{
    public class EstablishmentService: DatabaseServicesSchema<Establishment>, IEstablishmentService
    {
        public EstablishmentService(IDatabaseConnectorSettings settings) : base(settings)
        {
        }

        public virtual async Task<Establishment> GetByName(string name) 
        {
            IAsyncCursor<Establishment> collectionItem = await _collection.FindAsync<Establishment>(item => item.Name == name);
            return await collectionItem.FirstOrDefaultAsync();
        }
    }
}