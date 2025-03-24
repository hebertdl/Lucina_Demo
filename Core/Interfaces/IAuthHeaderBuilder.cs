using System.Net.Http.Headers;

namespace Core.Interfaces;

public interface IAuthHeaderBuilder
{
    Task<AuthenticationHeaderValue> BuildAuthHeaderAsync();
}