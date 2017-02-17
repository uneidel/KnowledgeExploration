using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CogServices.Entities
{
    public class KeyPhraseDocument
    {
        public string language { get; set; }
        public string id { get; set; }
        public string text { get; set; }
    }

    public class KeyPhraseEntity
    {
        public List<KeyPhraseDocument> documents { get; set; }
    }
}
