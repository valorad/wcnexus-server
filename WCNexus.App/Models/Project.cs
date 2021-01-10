using System.Collections.Generic;

namespace WCNexus.App.Models {
    public class Project: Nexus
    {
        public IList<Nexus> Techs { get; set; }
        public IList<Photo> Images { get; set; }
    }
}