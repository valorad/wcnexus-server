using System.Collections.Generic;

namespace WCNexus.App.Models
{

    public interface IProjectionOption
    {
        IEnumerable<string> Includes { get; set; }
        IEnumerable<string> Excludes { get; set; }
    }

    public interface IPaginationOption
    {
        int Page { get; set; }
        int PerPage { get; set; }
    }

    public interface ISortOption
    {
        string OrderBy { get; set; }
        string Order { get; set; }
    }

    public interface IDBViewOption : IProjectionOption, IPaginationOption, ISortOption { }

    public class ProjectionOption : IProjectionOption
    {
        public IEnumerable<string> Includes { get; set; }
        public IEnumerable<string> Excludes { get; set; }
    }

    public class PaginationOption : IPaginationOption
    {
        public int Page { get; set; }
        public int PerPage { get; set; }
    }

    public class SortOption : ISortOption
    {
        public string OrderBy { get; set; }
        public string Order { get; set; }
    }

    public class DBViewOption : IDBViewOption 
    {
        public IEnumerable<string> Includes { get; set; }
        public IEnumerable<string> Excludes { get; set; }
        public int Page { get; set; }
        public int PerPage { get; set; }
        public string OrderBy { get; set; }
        public string Order { get; set; }
    }


}