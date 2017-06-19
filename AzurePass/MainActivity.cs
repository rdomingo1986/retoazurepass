using Android.App;
using Android.Widget;
using Android.OS;

using Plugin.Geolocator;
using System;
using System.Json;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AzurePass
{
    public class CountryCodeResult
    {
        public string countryCode;
        public string countryName;
        public string distance;
        public string languages;
    }

    public class Country
    {
        public string continent;
        public string capital;
        public string languages;
        public string geonameId;
        public string south;
        public string isoAlpha3;
        public string north;
        public string fipsCode;
        public string population;
        public string east;
        public string isoNumeric;
        public string areaInSqKm;
        public string countryCode;
        public string west;
        public string countryName;
        public string continentName;
        public string currencyCode;

    }

    public class CountryInfoResult
    {
        [JsonProperty("geonames")]
        public Country[] country;
    }

    [Activity(Label = "AzurePass", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            //Set our view from the "main" layout resource
            SetContentView (Resource.Layout.Main);
            FindViewById<Button>(Resource.Id.ResolveChallenge).Click += (s, e) =>
            {
                ResolveChallenge();
            };
        }

        private async void ResolveChallenge()
        {
            try
            {
                var locator = CrossGeolocator.Current;
                locator.DesiredAccuracy = 50;
                var position = await locator.GetPositionAsync(timeoutMilliseconds: 10000);
                if (position == null)
                    return;

                string latitude = position.Latitude.ToString();
                string longitude = position.Longitude.ToString();

                Country country = await GetCountryInfo(latitude.Replace(",","."), longitude.Replace(",", "."));
                FindViewById<TextView>(Resource.Id.Country).Text = country.countryName;
                FindViewById<TextView>(Resource.Id.Capital).Text = country.capital;
                FindViewById<TextView>(Resource.Id.Continent).Text = country.continentName;
                FindViewById<TextView>(Resource.Id.AreaInSqKm).Text = country.areaInSqKm;
                FindViewById<TextView>(Resource.Id.Languages).Text = country.languages;
                FindViewById<TextView>(Resource.Id.Currency).Text = country.currencyCode;
            }
            catch (Exception e)
            {
                Android.Util.Log.Debug("Aplicación Geo Localización", e.ToString());
            }
        }

        private async Task<Country> GetCountryInfo(string latitude, string longitude)
        {
            string country = await GetCountryCode(latitude, longitude);
            string url = "http://api.geonames.org/countryInfoJSON?formatted=true&lang=es&country=" + country + "&username=rdomingo86&style=full";
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new Uri(url));
            request.ContentType = "application/json";
            request.Method = "GET";

            using (WebResponse response = await request.GetResponseAsync())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    JsonValue json = await Task.Run(() => JsonObject.Load(stream));
                    CountryInfoResult result = JsonConvert.DeserializeObject<CountryInfoResult>(json.ToString());
                    return result.country[0];
                }
            }
        }

        private async Task<string> GetCountryCode(string latitude, string longitude)
        {
            string url = "http://api.geonames.org/countryCodeJSON?formatted=true&lat="+ latitude + "&lng="+ longitude + "&username=rdomingo86&style=full";
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new Uri(url));
            request.ContentType = "application/json";
            request.Method = "GET";

            using (WebResponse response = await request.GetResponseAsync())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    JsonValue json = await Task.Run(() => JsonObject.Load(stream));
                    CountryCodeResult result = JsonConvert.DeserializeObject<CountryCodeResult>(json.ToString());
                    return result.countryCode.ToString();
                }
            }
        }

        
    }
}

