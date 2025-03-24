using Core.Models;
using FdaDrugEvent.Interfaces;

namespace FdaDrugEvent.Patterns;

public abstract class BaseFdaFilter : IFdaDrugEventFilters
{
    private IFdaDrugEventFilters? _nextFilter;

    public IFdaDrugEventFilters SetNext(IFdaDrugEventFilters nextFilter)
    {
        _nextFilter = nextFilter;
        return nextFilter;
    }

    public virtual FdaEvents ApplyFilter(FdaEvents input)
    {
        if (_nextFilter != null) return _nextFilter.ApplyFilter(input);
        return input;
    }
}