using Core.Models;

namespace FdaDrugEvent.Patterns;

public class FdaEventsBuilder
{
    private List<Patient> _patients = [];
    private DateTime _processDate = DateTime.MinValue;
    private DateTime _reportDate = DateTime.MinValue;
    private int _totalRecords;

    public FdaEventsBuilder WithProcessDate(DateTime processDate)
    {
        _processDate = processDate;
        return this;
    }

    public FdaEventsBuilder WithReportDate(DateTime reportDate)
    {
        _reportDate = reportDate;
        return this;
    }

    public FdaEventsBuilder WithTotalRecords(int totalRecords)
    {
        _totalRecords = totalRecords;
        return this;
    }

    public FdaEventsBuilder WithPatients(List<Patient> patients)
    {
        _patients = patients;
        return this;
    }

    public FdaEvents Build()
    {
        return new FdaEvents
        {
            ProcessDate = _processDate,
            ReportDate = _reportDate,
            TotalRecords = _totalRecords,
            Patients = _totalRecords != 0 ? _patients : null
        };
    }
}