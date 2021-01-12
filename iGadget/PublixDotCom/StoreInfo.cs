using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PublixDotCom.Util;
using System.Linq;

namespace PublixDotCom.StoreInfo
{
    public partial class StoreList
    {
        public static async Task<Store[]> Fetch(int zipCode)
        {
            return await Fetch($"{zipCode}");
        }

        public static async Task<Store[]> Fetch(string zipCode)
        {
            var url = @"https://services.publix.com/api/v1/storelocation?types=R,G,H,N,S&option=&count=15&includeOpenAndCloseDates=true&zipCode={0}";

            var requestUrl = string.Format(url, zipCode);

            var storesResponse = await WebClient.GetString(requestUrl, "application/json");

            var storeList = JsonConvert.DeserializeObject<StoreList>(storesResponse);

            if (storeList == null || storeList.Stores == null) 
            {
                return new Store[0];
            }
            
            return storeList.Stores.Where(a => string.IsNullOrWhiteSpace(a.Status)).ToArray();
        }


        [JsonProperty("Stores")]
        public Store[] Stores { get; set; }
    }

    public partial class Store
    {
        [JsonProperty("KEY")]
        public string Key { get; set; }

        [JsonProperty("NAME")]
        public string Name { get; set; }

        [JsonProperty("CLAT")]
        [JsonConverter(typeof(StringToFloatConverter))]
        public float Latitude { get; set; }

        [JsonProperty("CLON")]
        [JsonConverter(typeof(StringToFloatConverter))]
        public float Longitude { get; set; }

        [JsonProperty("ADDR")]
        public string Address { get; set; }

        [JsonProperty("CITY")]
        public string City { get; set; }

        [JsonProperty("STATE")]
        public string State { get; set; }

        [JsonProperty("ZIP")]
        public string ZIP { get; set; }

        [JsonProperty("PHONE")]
        public string Phone { get; set; }

        [JsonProperty("PHMPHONE")]
        public string PharmacyPhone { get; set; }

        [JsonProperty("LQRPHONE")]
        public string LiquorStorePhone { get; set; }

        [JsonProperty("PXFPHONE")]
        public string Pxfphone { get; set; }

        [JsonProperty("FAX")]
        public string Fax { get; set; }

        [JsonProperty("STRHOURS")]
        public string StoreHours { get; set; }

        [JsonProperty("PHMHOURS")]
        public string PharmacyHours { get; set; }

        [JsonProperty("LQRHOURS")]
        public string LiquorStoreHours { get; set; }

        [JsonProperty("PXFHOURS")]
        public string Pxfhours { get; set; }

        [JsonProperty("OPTION")]
        public string Option { get; set; }

        [JsonProperty("DEPTS")]
        public string Depts { get; set; }

        [JsonProperty("SERVICES")]
        public string Services { get; set; }

        [JsonProperty("TYPE")]
        public string Type { get; set; }

        [JsonProperty("UNIQUE")]
        public string Unique { get; set; }

        [JsonProperty("EPPH")]
        public string Epph { get; set; }

        [JsonProperty("CSPH")]
        public string Csph { get; set; }

        [JsonProperty("MAPH")]
        public string Maph { get; set; }

        [JsonProperty("DISTANCE")]
        [JsonConverter(typeof(StringToLongConverter))]
        public long? Distance { get; set; }

        [JsonProperty("WABREAK")]
        [JsonConverter(typeof(StringToLongConverter))]
        public long? Wabreak { get; set; }

        [JsonProperty("WASTORENUM")]
        [JsonConverter(typeof(StringToLongConverter))]
        public long? Wastorenum { get; set; }

        [JsonProperty("OPENINGDATE")]
        public string OpeningDate { get; set; }

        [JsonProperty("CLOSINGDATE")]
        public object ClosingDate { get; set; }

        [JsonProperty("ISENABLED")]
        public bool? IsEnabled { get; set; }

        [JsonProperty("STOREDATETIME")]
        public DateTimeOffset? StoreDateTime { get; set; }

        [JsonProperty("STATUS")]
        public string Status { get; set; }

        [JsonProperty("STOREMAPSID")]
        public long? Storemapsid { get; set; }

        [JsonProperty("STOREMAPTOGGLE")]
        public bool? Storemaptoggle { get; set; }

        [JsonProperty("IMAGE")]
        public Image Image { get; set; }

        [JsonProperty("SHORTNAME")]
        public string ShortName { get; set; }
    }

    public partial class Image
    {
        [JsonProperty("Thumbnail")]
        public Uri[] Thumbnail { get; set; }

        [JsonProperty("Hero")]
        public Uri[] FullSize { get; set; }
    }
}
