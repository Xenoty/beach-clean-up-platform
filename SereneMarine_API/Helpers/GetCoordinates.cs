using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using WebApi.Models.Events;

namespace WebApi.Helpers
{
    public class GetCoordinates
    {
        public async Task<EventCoordinatesModel> GetLongLatMapBox(string address)
        {
            //initalize variables required
            HttpClient client = new HttpClient();
            HttpResponseMessage response = null;
            EventCoordinatesModel ecm = new EventCoordinatesModel();

            //UPLOAD URL
            string access_token = "pk.eyJ1IjoieGVub3R5IiwiYSI6ImNrNmc5aDJ6NDF0Z3IzbG12cXJqOTdhenkifQ.bUvMdh0ICYpAjs5Vi0_3Bw";
            string baseUrl = "https://api.mapbox.com/geocoding/v5/mapbox.places/";
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
