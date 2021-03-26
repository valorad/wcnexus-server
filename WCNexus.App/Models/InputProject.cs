using System.Collections.Generic;

namespace WCNexus.App.Models {
    public class InputProject: InputNexus
    {
        public IEnumerable<Nexus> Techs { get; set; }
        public IEnumerable<Photo> Images { get; set; }
    }
}