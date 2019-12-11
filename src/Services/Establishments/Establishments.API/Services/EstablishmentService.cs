using System.Threading.Tasks;
using CommonLibrary.DataAccess;
using CommonLibrary.DataAccess.Abstractions;
using Establishments.API.Models;
using MongoDB.Driver;

namespace Establishments.API.Services
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