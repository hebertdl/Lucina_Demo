using Core.Models;

namespace FdaDrugEvent.Filters;

public class FdaEventsFirst10Filter : BaseFdaFilter
{
    public override FdaEvents ApplyFilter(FdaEvents fdaEvents)
    {
        if (fdaEvents.Patients == null) return base.ApplyFilter(fdaEvents);
        fdaEvents.Patients = fdaEvents.Patients.Take(10).ToList();
        fdaEvents.TotalRecords = fdaEvents.Patients.Count;

        return base.ApplyFilter(fdaEvents);
    }
}