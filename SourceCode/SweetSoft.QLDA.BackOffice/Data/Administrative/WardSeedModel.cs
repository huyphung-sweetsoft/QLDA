using Newtonsoft.Json;

namespace SweetSoft.QLDA.BackOffice.Data.Administrative
{
    public class WardSeedModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public WardNameModel Name { get; set; }

        [JsonProperty("code")]
        public WardCodeModel Code { get; set; }

        [JsonProperty("parent")]
        public WardParentModel Parent { get; set; }
    }

    public class WardNameModel
    {
        [JsonProperty("local")]
        public string Local { get; set; }

        [JsonProperty("en")]
        public string English { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }
    }

    public class WardCodeModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class WardParentModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public WardParentNameModel Name { get; set; }
    }

    public class WardParentNameModel
    {
        [JsonProperty("local")]
        public string Local { get; set; }
    }
}