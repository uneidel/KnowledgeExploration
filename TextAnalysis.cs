using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CogServices
{
    internal class TextAnalysis
    {
        RestSharp.RestClient c = new RestSharp.RestClient();


        public void PhraseExtraction() { }
    }

    public class Document
    {
        public string language { get; set; } // de, en
        public string id { get; set; } // needs to have an unique Ident e.g. Guid.NewGuid().ToString();
        public string text { get; set; } // Plain Text
    }

    public class TextAnalysisCollection
    {
        public List<Document> documents { get; set; }
    }
}
