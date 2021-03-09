using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using WebApi.Models.Events;

namespace WebApi.Helpers
{
    public class GetCoordinates
    {
        private readonly IConfiguration _configuration;

        public GetCoordinates(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<EventCoordinatesModel> GetLongLatMapBox(string address)
        {
            //initalize variables required
            HttpClient client = new HttpClient();
            HttpResponseMessage response = null;
            EventCoordinatesModel ecm = new EventCoordinatesModel();

            //UPLOAD URL
            string access_token = _configuration.GetValue<string>("AppSettings:MapboxAPIkey");
            string baseUrl = _configuration.GetValue<string>("AppSettings:MapBoxPlacesUrl");
            string jsonExt = ".json?access_token=";
            string url = baseUrl + address + jsonExt + access_token;

            //RESPONSE from api
            response = await client.GetAsync(url);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return ecm;
            }

            //READING DATA FROM JSON RESPONSE 
            string jsonString = await response.Content.ReadAsStringAsync();
            //assign to dynamic obj to access specific elements
            dynamic obj = JsonConvert.DeserializeObject(jsonString);

            //check country location first, if not south africa then don't store coordinates
            dynamic country = obj.features[0].place_name;

            if (!country.ToString().Contains("South Africa"))
            {
                return ecm;
            }

            dynamic coordinates = obj.features[0].geometry.coordinates;
            //assign values to model
            ecm.latitude = coordinates[0];
            ecm.longitude = coordinates[1];

            return ecm;
        }
    }
}
