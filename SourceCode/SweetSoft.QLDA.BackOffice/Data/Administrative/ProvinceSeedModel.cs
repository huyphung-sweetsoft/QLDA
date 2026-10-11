using Newtonsoft.Json;

namespace SweetSoft.QLDA.BackOffice.Data.Administrative
{
    public class ProvinceSeedModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public ProvinceNameModel Name { get; set; }

        [JsonProperty("code")]
        public ProvinceCodeModel Code { get; set; }
    }

    public class ProvinceNameModel
    {
        [JsonProperty("local")]
        public string Local { get; set; }

        [JsonProperty("en")]
        public string English { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }
    }

    public class ProvinceCodeModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}