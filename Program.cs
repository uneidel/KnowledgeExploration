using System;
using System.Net.Http.Headers;
using System.Text;
using System.Net.Http;
using System.Web;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.SharePoint.Client;
using System.Security;
using Newtonsoft.Json.Linq;
using System.Net;
using RestSharp;
using System.Linq;

using CogServices.Entities;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace CSHttpClientSample
{

    static class Program
    {
        static bool DEBUG = true;

        static void Main()
        {
            MainAsync().Wait();
        }
        static async Task MainAsync()
        {
            string kespath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "kes", "requests.data");
            string kesindex = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "kes", "requests.index");
            DeleteKESDataIfExists(kespath);
            DeleteKESDataIfExists(kesindex);

            const string password = "";
            var securePassword = new SecureString();
            foreach (var c in password)
            {
                securePassword.AppendChar(c);
            }
            var credentials = new SharePointOnlineCredentials("", securePassword);
            Log("Cognitive Services - Knowledge Exploration API v0.1");
            Log("Press any key to start");
            Console.ReadKey();
            Log("Querying SharePoint");
            var items = GetSPItems("https://microsoft.sharepoint.com/teams/DX_TE_ISV", credentials, "TERequest");

            //Shitty SP REST API
            List<SPItem> spitems = new List<SPItem>();
            foreach (var itm in items)
            {
                SPItem sp = new SPItem();
                sp.Assigned_TE = Convert.ToString(itm["Assigned_TE"]);

                sp.Engagement_Type = Convert.ToString(itm["Engagement_Type"]);
                sp.General_PartnerTyp = Convert.ToString(itm["General_PartnerTyp"]);
                sp.General_SupportingPartner = Convert.ToString(itm["General_SupportingPartner"]).Trim();
                sp.Requestor = Convert.ToString(itm["Requestor"]);
                sp.Title = Convert.ToString(itm["Title"]);
                sp.General_Summary = Convert.ToString(itm["General_Summary"]);
                sp.Id = Convert.ToString(itm["Id"]);
                sp.Guid = Convert.ToString(itm["GUID"]);
                spitems.Add(sp);
            }

            List<KeyPhraseDocument> docsfortext = new List<KeyPhraseDocument>();
            foreach (var itm in items)
            {
                var Document = new KeyPhraseDocument();
                Document.text = Convert.ToString(itm["General_Summary"]);
                Document.id = Convert.ToString(itm["GUID"]);
                Document.language = "de";
                docsfortext.Add(Document);
            }

            var package = new KeyPhraseEntity() { documents = docsfortext };
            Log("Sending data to Text Analysis API");
            var data = await MakeRequest(package);

            Log("Building Kes Data Package");
            //Working on KesData
            StreamWriter sw = System.IO.File.AppendText(kespath);
            foreach (var doc in data.documents)
            {
                KESObject kes = new KESObject();
                List<Engagement> engagements = new List<Engagement>();
                List<Partner> partners = new List<Partner>();

                var id = Convert.ToString(doc["id"]);
                var o = spitems.FirstOrDefault(x => x.Guid == id);
                var keyPhrases = doc["keyPhrases"];
                List<string> nkey = new List<string>();
                foreach (var k in keyPhrases)
                    nkey.Add(k.Value);
                Engagement engagement = new Engagement();
                engagement.Type = o.Engagement_Type;
                engagement.Keywords = nkey;
                engagements.Add(engagement);
                Partner p = new Partner();
                p.Name = o.Title;
                p.Type = o.General_PartnerTyp;
                partners.Add(p);

                kes.PBE = o.Requestor;
                kes.TE = !String.IsNullOrEmpty(o.Assigned_TE) ? o.Assigned_TE : "notAssigned";

                kes.SupportingPartner = !String.IsNullOrEmpty(o.General_SupportingPartner)? o.General_SupportingPartner : "notAssigned";

                kes.Engagement = engagements;
                kes.Partner = partners;
                kes.UniqueId = o.Guid;

                var json = JsonConvert.SerializeObject(kes);
                sw.WriteLine(json);
            }
            sw.Dispose();

            Log("Building KES IndexFile");
            await BuildIndexAsync();

            Log("Compile KES Grammar");
            await CompileGrammarAsync();


            Log("Starting local KES Server");
            await StartLocalKESInstance();
            Console.ReadLine();
        }
        static Task StartLocalKESInstance()
        {
            // there is no non-generic TaskCompletionSource
            //kes host_service requests.grammar requests.index --port 8000
            var tcs = new TaskCompletionSource<bool>();
            var fileName = @"C:\Program Files\Microsoft\Knowledge Exploration Service\kes.exe";
            string kesgrammar = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "kes", "requests.grammar");
            string kesindex = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "kes", "requests.index");
            var process = new Process
            {
                StartInfo = { FileName = fileName, Arguments=$" host_service {kesgrammar} {kesindex}  --port 8000"},
                EnableRaisingEvents = true
            };

            process.Exited += (sender, args) =>
            {
                tcs.SetResult(true);
                process.Dispose();
            };

            process.Start();

            return tcs.Task;
        }

        static Task BuildIndexAsync()
        {
            // there is no non-generic TaskCompletionSource
            var tcs = new TaskCompletionSource<bool>();
            var fileName = @"C:\Program Files\Microsoft\Knowledge Exploration Service\kes.exe";
            string kesdata = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "kes", "requests.data");
            string kesschema = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "kes", "requests.schema");
            string kesindex = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "kes", "requests.index");
            var process = new Process
            {
                StartInfo = { FileName = fileName, Arguments = $" build_index {kesschema} {kesdata} {kesindex} --overwrite 1" },
                EnableRaisingEvents = true
            };

            process.Exited += (sender, args) =>
            {
                tcs.SetResult(true);
                process.Dispose();
            };

            process.Start();

            return tcs.Task;
        }
        static Task CompileGrammarAsync()
        {
            // there is no non-generic TaskCompletionSource
            var tcs = new TaskCompletionSource<bool>();
            var fileName = @"C:\Program Files\Microsoft\Knowledge Exploration Service\kes.exe";
            string kesgrammarin = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "kes", "requests.xml");
            string kesgrammarout = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "kes", "requests.grammar");

            var process = new Process
            {
                StartInfo = { FileName = fileName, Arguments = $" build_grammar {kesgrammarin} {kesgrammarout} --overwrite 1 " },
                EnableRaisingEvents = true

            };

            process.Exited += (sender, args) =>
            {
                tcs.SetResult(true);
                process.Dispose();
            };

            process.Start();

            return tcs.Task;
        }
        private static void DeleteKESDataIfExists(string kespath)
        {
            if (System.IO.File.Exists(kespath))
                System.IO.File.Delete(kespath);
        }

        public static JArray GetSPItems(string baseuri, ICredentials credentials, string listTitle)
        {
            using (var client = new WebClient())
            {
                client.Headers.Add("X-FORMS_BASED_AUTH_ACCEPTED", "f");
                client.Credentials = credentials;
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json;odata=verbose");
                client.Headers.Add(HttpRequestHeader.Accept, "application/json;odata=verbose");
                var endpointUri = new Uri(
                    String.Format("{0}/_api/web/lists/getbytitle('{1}')/items?$select=Id,GUID,Title,General_PartnerTyp,General_Summary,Engagement_Type,Assigned_TE, Requestor,General_SupportingPartner", baseuri, listTitle));
                var result = client.DownloadString(endpointUri);
                var x1 = JObject.Parse(result)["d"].ToString();
                var t = JsonConvert.DeserializeObject<dynamic>(x1);
                t = t.results;
                return t;
            }
        }
        static void Log(string message)
        {
            if (DEBUG)
            {
                var timeStamp = DateTime.Now.ToString("HH.mm.ss.fff");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{timeStamp}: {message}");
                Console.ForegroundColor = ConsoleColor.White;
            }

        }

        static async Task<dynamic> MakeRequest(KeyPhraseEntity r )
        {
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            // Request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "8a0b472bdce448baa4fe7cf4cc60426e");

            var uri = "https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/keyPhrases";

            HttpResponseMessage response;


            var json = JsonConvert.SerializeObject(r);

            byte[] byteData = Encoding.UTF8.GetBytes(json);


            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await client.PostAsync(uri, content);
            }
            var byties = await response.Content.ReadAsByteArrayAsync();
            var responsestuff = Encoding.UTF8.GetString(byties);
            var data = JsonConvert.DeserializeObject(responsestuff);
            return data;
        }
    }
}
