using Core.Models;
using FdaDrugEvent.Patterns;

namespace FdaDrugEvent.Filters;

public class FdaEventsFirst10Filter : BaseFdaFilter
{
    public override FdaEvents ApplyFilter(FdaEvents fdaEvents)
    {
        if (fdaEvents.Patients == null) return base.ApplyFilter(fdaEvents);
        fdaEvents.Patients = fdaEvents.Patients.Take(5).ToList();
        fdaEvents.TotalRecords = fdaEvents.Patients.Count;

        return base.ApplyFilter(fdaEvents);
    }
}