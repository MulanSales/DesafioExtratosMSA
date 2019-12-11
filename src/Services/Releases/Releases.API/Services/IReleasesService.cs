using System.Threading.Tasks;
using CommonLibrary.DataAccess;
using Releases.API.Models;

namespace Releases.API.Services
{
    public interface IReleasesService : IDatabaseServicesSchema<Release>
    {
    }
}