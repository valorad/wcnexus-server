using System.Collections.Generic;

namespace WCNexus.App.Models
{
    public interface INexus
    {
        string DBName { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        string URL { get; set; }
        string Cover { get; set; }
        string Avatar { get; set; }
        IEnumerable<string> Photos { get; set; }
        string Type { get; set; }
    }

}
