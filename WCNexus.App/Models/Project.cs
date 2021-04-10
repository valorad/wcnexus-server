using System.Collections.Generic;

namespace WCNexus.App.Models {
    public class Project: Nexus, IProject
    {
        public IEnumerable<Nexus> Techs { get; set; }
        public IEnumerable<Photo> Images { get; set; }
    }
}