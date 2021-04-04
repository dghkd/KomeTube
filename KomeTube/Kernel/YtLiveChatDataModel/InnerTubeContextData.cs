using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace KomeTube.Kernel.YtLiveChatDataModel
{
    public class Client
    {
        public string hl { get; set; }
        public string gl { get; set; }
        public string remoteHost { get; set; }
        public string deviceMake { get; set; }
        public string deviceModel { get; set; }
        public string visitorData { get; set; }
        public string userAgent { get; set; }
        public string clientName { get; set; }
        public string clientVersion { get; set; }
        public string osName { get; set; }
        public string osVersion { get; set; }
        public string originalUrl { get; set; }
        public string platform { get; set; }
        public string clientFormFactor { get; set; }
        public string browserName { get; set; }
        public string browserVersion { get; set; }
    }

    public class User
    {
        //public bool lockedSafetyMode { get; set; }
    }

    public class Request
    {
        public bool useSsl { get; set; }
    }

    public class ClickTracking
    {
        public string clickTrackingParams { get; set; }
    }

    public class INNERTUBE_CONTEXT
    {
        public INNERTUBE_CONTEXT()
        {
            this.client = new Client();
            this.user = new User();
            this.request = new Request();
            this.clickTracking = new ClickTracking();
        }

        public Client client { get; set; }
        public User user { get; set; }
        public Request request { get; set; }
        public ClickTracking clickTracking { get; set; }
    }

    public class InnerTubeContextData
    {
        public InnerTubeContextData()
        {
            this.context = new INNERTUBE_CONTEXT();
            this.continuation = "";
        }

        public INNERTUBE_CONTEXT context { get; set; }
        public string continuation { get; set; }

        [JsonIgnore]
        public string INNERTUBE_API_KEY { get; set; }

        public override string ToString()
        {
            string ret = JsonConvert.SerializeObject(this);
            return ret;
        }
    }
}