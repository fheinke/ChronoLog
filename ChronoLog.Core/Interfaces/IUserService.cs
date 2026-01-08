namespace ChronoLog.Core.Interfaces;

public interface IUserService
{
    Task<string?> GetUserNameAsync();
    Task<string?> GetUserEmailAsync();
    Task<string?> GetUserIdAsync();
}