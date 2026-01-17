namespace UserManagement.Core.Interfaces;

public interface IPasswordValidator
{
    (bool IsValid, string? ErrorMessage) Validate(string password, string email, string? phoneNumber);
}
