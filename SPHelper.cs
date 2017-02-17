using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SPTester
{
    internal class SPHelper
    {
        private RestClient client = null;
        private string accesstoken = String.Empty;
        public SPHelper(string accesstoken)
        {
            
            //this.accesstoken = accesstoken;
            this.client = new RestClient("https://graph.microsoft.com/v1.0/");
        }
        private RestClient Client
        {
            get
            {
                return client;
            }
        }
        private void AddHeader(RestRequest r)
        {
            r.AddParameter("User-Agent", "DXBOT/1.0", RestSharp.ParameterType.HttpHeader);
            r.AddParameter("Content-Type", "application/x-www-form-urlencoded", ParameterType.HttpHeader);
            r.AddParameter("client-request-id", Guid.NewGuid().ToString(), ParameterType.HttpHeader); //https://blogs.msdn.microsoft.com/exchangedev/2014/03/25/using-oauth2-to-access-calendar-contact-and-mail-api-in-office-365-exchange-online/
            r.AddParameter("return-client-request-id", "true", ParameterType.HttpHeader);
            r.AddParameter("Authorization", string.Format("Bearer " + accesstoken), ParameterType.HttpHeader);
        }
        public async Task<JArray> GetProjectByMail(string mailaddress)
        {
           
            client.BaseUrl = new Uri("https://microsoft.sharepoint.com/teams/DX_TE_ISV/");
            string url = $"_api/web/Lists/GetbyTitle('TERequest')/Items?$filter=Assigned_TE+eq+'{mailaddress}'";
            RestRequest r = new RestSharp.RestRequest(url, RestSharp.Method.GET);
            AddHeader(r);
            var response = await Client.ExecuteTaskAsync(r);
            JArray projects = null;
            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
                projects = Newtonsoft.Json.JsonConvert.DeserializeObject<JArray>(response.Content);
            return projects;
        }

        public async Task<JObject> GetAverageAssignmentDurationForTimespan(TimeSpan span)
        {
            try { 
            client.BaseUrl = new Uri("[URLTOSP]");
            var pointofTime = GetISO8601DateTimeString(DateTime.Now.Add(span));
            string url = $"_api/web/Lists/GetByTitle('TERequest')/Items?$filter=Created+gt+'{pointofTime}'";
            RestRequest r = new RestSharp.RestRequest(url, RestSharp.Method.GET);
               r.AddHeader("X-FORMS_BASED_AUTH_ACCEPTED", "f");
                r.AddHeader(HttpRequestHeader.ContentType.ToString(), "application/json;odata=verbose");
                r.AddHeader(HttpRequestHeader.Accept.ToString(), "application/json;odata=verbose");
                r.Credentials = new System.Net.NetworkCredential("[TODO]", "[TBODO]");
                
                var response = await Client.ExecuteTaskAsync(r);
            JArray projects = null;
            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
                projects = Newtonsoft.Json.JsonConvert.DeserializeObject<JArray>(response.Content);
            }
            catch(Exception ex)
            {
                var foo = ex;
            }
            return null;
        }
        private string GetISO8601DateTimeString(DateTime time)
        {
            return time.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffff");
        }


    }
}
