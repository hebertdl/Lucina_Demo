using Core.Models;

namespace FdaDrugEvent.Patterns;

public class FdaEventsBuilder
{
    private List<Patient> _patients;
    private DateTime _processDate;
    private DateTime _reportDate;
    private int _totalRecords;

    public FdaEventsBuilder()
    {
        _processDate = DateTime.MinValue;
        _reportDate = DateTime.MinValue;
        _totalRecords = 0;
        _patients = null;
    }

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