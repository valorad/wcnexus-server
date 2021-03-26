using System.Collections.Generic;

namespace WCNexus.App.Models {
    public class Project: Nexus
    {
        public IEnumerable<Nexus> Techs { get; set; }
        public IEnumerable<Photo> Images { get; set; }
    }
}