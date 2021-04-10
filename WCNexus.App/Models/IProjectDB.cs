using System.Collections.Generic;

namespace WCNexus.App.Models
{
    public interface IProjectDB
    {
        string DBName { get; set; }
        IEnumerable<string> Techs { get; set; }
        IEnumerable<string> Images { get; set; }
    }

}