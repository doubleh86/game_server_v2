using Microsoft.AspNetCore.Identity;

namespace ServerFramework.CommonUtils.Helper;

public static class PasswordHelper
{
    public static string PasswordHash(string id, string password)
    {
        var passwordHasher = new PasswordHasher<string>();
        var passwordHashed = passwordHasher.HashPassword(id, password);

        return passwordHashed;
    }

    public static bool CheckPassword(string id, string password, string passwordDb)
    {
        var passwordHasher = new PasswordHasher<string>();
        var verificationResult = passwordHasher.VerifyHashedPassword(id, passwordDb, password);
        if (verificationResult == PasswordVerificationResult.Success)
            return true;

        return false;
    }
}