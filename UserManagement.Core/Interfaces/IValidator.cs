namespace UserManagement.Core.Interfaces;

public interface IValidator<T>
{
    Task<(bool IsValid, string? ErrorMessage)> ValidateAsync(T context);
}
