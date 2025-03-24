using Core.Models;

namespace FdaDrugEvent.Filters;

public class FdaEventsWomanFilter : BaseFdaFilter
{
    public override FdaEvents ApplyFilter(FdaEvents fdaEvents)
    {
        if (fdaEvents.Patients == null) return base.ApplyFilter(fdaEvents);
        fdaEvents.Patients = fdaEvents.Patients.Where(x => x.PatientSex == "2").ToList();
        fdaEvents.TotalRecords = fdaEvents.Patients.Count;

        return base.ApplyFilter(fdaEvents);
    }
}