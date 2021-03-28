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

}