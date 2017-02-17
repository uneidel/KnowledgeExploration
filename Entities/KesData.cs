using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CogServices.Entities
{
    public class Partner
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }

    public class Engagement
    {
        //public string Name { get; set; }
        public List<string> Keywords { get; set; }
        public string Type { get; set; }
    }

    public class KESObject
    {
        public List<Partner> Partner { get; set; }
        public List<Engagement> Engagement { get; set; }
        public string SupportingPartner { get; set; }
        public string PBE { get; set; }
        public string TE { get; set; }
        public string UniqueId { get; set; }
    }
}
