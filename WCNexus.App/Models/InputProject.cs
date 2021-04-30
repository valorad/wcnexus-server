using System.Collections.Generic;

namespace WCNexus.App.Models {
    public class InputProject: InputNexus
    {
        public IEnumerable<string> Techs { get; set; }
        public IEnumerable<string> Images { get; set; }
        public InputProject()
        {
            Techs = new List<string>();
            Images = new List<string>();
        }
    }
}