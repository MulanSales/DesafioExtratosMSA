using System.Threading.Tasks;
using CommonLibrary.DataAccess;
using CommonLibrary.DataAccess.Abstractions;
using MongoDB.Driver;
using Statements.API.Models;

namespace Statements.API.Services
{
    public class ReleasesService : DatabaseServicesSchema<Release>, IReleasesService
    {
        public ReleasesService(IDatabaseConnectorSettings settings) : base(settings)
        {
        }
    }
}