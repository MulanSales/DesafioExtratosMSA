using System.Threading.Tasks;
using CommonLibrary.DataAccess;
using Statements.API.Models;

namespace Statements.API.Services
{
    public interface IEstablishmentService : IDatabaseServicesSchema<Establishment>
    {
        Task<Establishment> GetByName(string name);
    }
}