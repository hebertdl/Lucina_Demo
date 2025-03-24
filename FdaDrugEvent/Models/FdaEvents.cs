using System.Diagnostics.CodeAnalysis;

namespace Core.Models;

[ExcludeFromCodeCoverage]
public class FdaEvents
{
    public DateTime ProcessDate { get; set; }
    public DateTime ReportDate { get; set; }
    public int TotalRecords { get; set; }
    public List<Patient>? Patients { get; set; }
}