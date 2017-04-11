using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LNF.Models.Scheduler;

namespace CacheUtility.Models
{
    public class ResourcesModel
    {
        public bool Active { get; set; }
        public string ResourceName { get; set; }
        public string BuildingName { get; set; }
        public string LabDisplayName { get; set; }
        public string ProcessTechName { get; set; }
        public IEnumerable<ResourceModel> Items { get; set; }
    }
}