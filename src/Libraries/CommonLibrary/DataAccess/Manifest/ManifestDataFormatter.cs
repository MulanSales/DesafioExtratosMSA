using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CommonLibrary.DataAccess.Manifest 
{
    public static class ManifestDataFormatter
    {
        public async static Task<T> ParseManifestDataToObject<T>(string embeddedFilePath) where T : class
        {
            var assembly = Assembly.GetEntryAssembly();
            var resourceStream = assembly.GetManifestResourceStream(embeddedFilePath);

            string result = "";
            using (var reader = new StreamReader(resourceStream, Encoding.UTF8))
            {
                string currChunck = await reader.ReadToEndAsync();
                result = result + currChunck;
            }

            return JsonConvert.DeserializeObject<T>(result);
        }
    }
}