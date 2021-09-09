using System.Collections.Generic;

namespace WCNexus.App.Models {
    public class InputNexus
    {
        public string DBName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string URL { get; set; }
        public string Cover { get; set; }
        public string Avatar { get; set; }
        public IEnumerable<string> Photos { get; set; }
        public string Type { get; set; }
    }
}