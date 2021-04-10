using System.Collections.Generic;

namespace WCNexus.App.Models {
    public interface IProject: INexus
    {
        public IEnumerable<Nexus> Techs { get; set; }
        public IEnumerable<Photo> Images { get; set; }
    }
}