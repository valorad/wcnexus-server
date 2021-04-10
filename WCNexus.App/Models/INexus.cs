namespace WCNexus.App.Models
{
    public interface INexus
    {
        string DBName { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        string URL { get; set; }
        string Logo { get; set; }
        string Type { get; set; }
    }

}


