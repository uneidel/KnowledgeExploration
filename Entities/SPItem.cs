using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CogServices.Entities
{
    [JsonObject]
    public class SPItem
    {
        public string Guid { get; set; }
        public string Id { get; set; }
        public string Title { get; set; }
        public string General_PartnerTyp { get; set; }
        public string General_Summary { get; set; }
        public string Engagement_Type { get; set; }
        public string Assigned_TE { get; set; }
        public string Requestor { get; set; }
        public string General_SupportingPartner { get; set; }
    }
}
