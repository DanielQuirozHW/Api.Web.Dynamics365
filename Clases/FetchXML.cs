using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Api.Web.Dynamics365.Clases
{
    public class FetchXML
    {
        [JsonProperty("@odata.context")]
        public string Context { get; set; }

        [JsonProperty("@@odata.count")]
        public string Count { get; set; }
        [JsonProperty("@Microsoft.Dynamics.CRM.totalrecordcount")]
        public string Totalrecordcount { get; set; }
        [JsonProperty("@Microsoft.Dynamics.CRM.totalrecordcountlimitexceeded")]
        public string Totalrecordcountlimitexceeded { get; set; }
        [JsonProperty("@Microsoft.Dynamics.CRM.globalmetadataversion")]
        public string Globalmetadataversion { get; set; }
        [JsonProperty("@Microsoft.Dynamics.CRM.fetchxmlpagingcookie")]
        public string Fetchxmlpagingcookie { get; set; }
        [JsonProperty("@Microsoft.Dynamics.CRM.morerecords")]
        public bool Morerecords { get; set; }
        public JArray Value { get; set; }
    }
}
