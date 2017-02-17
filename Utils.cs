using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Deserializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CogServices
{
    public class SharePointJsonDeserializer : IDeserializer
    {
        public T Deserialize<T>(IRestResponse response)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(JObject.Parse(response.Content)["d"].ToString());
            }
            catch (Exception exception)
            {
                //log exception
                Console.WriteLine(exception.Message);
            }
            return default(T);
        }
        public string RootElement { get; set; }
        public string Namespace { get; set; }
        public string DateFormat { get; set; }
    }
}

