using Core.Models;

namespace FdaDrugEvent.Interfaces;

public interface IFdaDrugEventFilters
{
    IFdaDrugEventFilters SetNext(IFdaDrugEventFilters nextFilter);
    FdaEvents ApplyFilter(FdaEvents input);
}