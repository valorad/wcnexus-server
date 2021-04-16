using System.Collections.Generic;

namespace WCNexus.App.Models
{
    public interface IStoredProject
    {
        string DBName { get; set; }
        IEnumerable<string> Techs { get; set; }
        IEnumerable<string> Images { get; set; }
    }

}