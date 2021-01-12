using System;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Newtonsoft.Json;
using PublixDotCom.StoreInfo;
using PublixDotCom.Util;

namespace PublixDotCom.Promotions
{
    public partial class Welcome
    {
        [JsonProperty("promotion")]
        public Promotion Promotion { get; set; }

        public static async Task<Promotion> Fetch(Store store)
        {
            string PublixAdQuery = @"
            query Promotion($promotionCode: ID, $previewHash: String, $promotionTypeID: Int, $require: String, $nuepOpen: Boolean) {
                promotion(code: $promotionCode, imageWidth: 1400, previewHash: $previewHash, promotionTypeID: $promotionTypeID, require: $require) {
                    id
                    title
                    displayOrder
                    saleStartDateString
                    saleEndDateString
                    postStartDateString
                    previewPostStartDateString
                    code
                    rollovers(previewHash: $previewHash, require: $require) {
                        id
                        title
                        deal
                        isCoupon
                        buyOnlineLinkURL
                        imageCommon: imageURL(imageWidth: 600, previewHash: $previewHash, require: $require)
                    }
                    pages(imageWidth: 1400, previewHash: $previewHash, require: $require, nuepOpen: $nuepOpen) {
                        id
                        imageURL(previewHash: $previewHash,require: $require)
                        order
                    }
                }
            }";

            var promoGraphQLServer = "https://graphql-cdn-slplatform.liquidus.net/";

            var graphQLClient = new GraphQLHttpClient(promoGraphQLServer, new NewtonsoftJsonSerializer());
            graphQLClient.HttpClient.DefaultRequestHeaders.Add("campaignid", "80db0669da079dc6");
            graphQLClient.HttpClient.DefaultRequestHeaders.Add("storeref", store.Key);
            graphQLClient.HttpClient.DefaultRequestHeaders.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue() { NoCache = true };
            graphQLClient.HttpClient.DefaultRequestHeaders.Pragma.ParseAdd("No-Cache");
            graphQLClient.HttpClient.DefaultRequestHeaders.Add("authority", new Uri(promoGraphQLServer).Host);
            graphQLClient.HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd(WebClient.UserAgent);

            var graphQLResponse = await graphQLClient.SendQueryAsync<PublixDotCom.Promotions.Welcome>(new GraphQLRequest
            {
                Query = PublixAdQuery,
                OperationName = "Promotion",
                Variables = new
                {
                    sort = "",
                    preload = 3,
                    disablesneakpeekhero = false,
                    countryid = 1,
                    languageid = 1,
                    env = "undefined",
                    storeref = store.Key,
                    storeid = "undefined",
                    campaignid = "80db0669da079dc6",
                    require = "",
                    nuepOpen = false
                }
            });
            return graphQLResponse.Data.Promotion;
        }
    }

    public partial class Promotion
    {
        [JsonProperty("id")]
        [JsonConverter(typeof(StringToIntConverter))]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("displayOrder")]
        public int DisplayOrder { get; set; }

        [JsonProperty("saleStartDateString")]
        [JsonConverter(typeof(PublixDateConverter))]
        public DateTime SaleStartDate { get; set; }

        [JsonProperty("saleEndDateString")]
        [JsonConverter(typeof(PublixDateConverter))]
        public DateTime SaleEndDate { get; set; }

        [JsonProperty("postStartDateString")]
        [JsonConverter(typeof(PublixDateConverter))]
        public DateTime PostStartDate { get; set; }

        [JsonProperty("previewPostStartDateString")]
        [JsonConverter(typeof(PublixDateConverter))]
        public DateTime PreviewPostStartDate{ get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("pages")]
        public Page[] Pages { get; set; }

        [JsonProperty("rollovers")]
        public Rollover[] Rollovers { get; set; }
    }

    public partial class Page
    {
        [JsonProperty("id")]
        [JsonConverter(typeof(StringToIntConverter))]
        public int Id { get; set; }

        [JsonProperty("imageURL")]
        public Uri ImageUrl { get; set; }

        [JsonProperty("order")]
        [JsonConverter(typeof(StringToIntConverter))]
        public int Order { get; set; }
    }

    public class Rollover
    {
        [JsonProperty("id")]
        [JsonConverter(typeof(StringToIntConverter))]
        public int ID { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("deal")]
        public string Deal { get; set; }
    }


}
