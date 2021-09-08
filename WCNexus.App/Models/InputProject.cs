using System.Collections.Generic;

namespace WCNexus.App.Models {
    public class InputProject: InputNexus
    {
        public IEnumerable<string> Techs { get; set; }
    }
}