using Core.Models;

namespace FdaDrugEvent.Interfaces;

public interface IFdaDrugEventExtractor
{
    Task<FdaEvents> ConvertToFdaEvents(string rawData, DateTime reportDate);
}