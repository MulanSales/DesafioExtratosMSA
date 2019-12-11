using System.Threading.Tasks;
using CommonLibrary.DataAccess;
using Establishments.API.Models;

namespace Establishments.API.Services
{
    public interface IEstablishmentService : IDatabaseServicesSchema<Establishment>
    {
        Task<Establishment> GetByName(string name);
    }
}