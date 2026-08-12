using BoulderingRecordAPI.Validation;

namespace BoulderingRecordAPI.Tests.Validation;

public class PasswordPolicyTests
{
    [Theory]
    [InlineData("Password123!")]
    [InlineData("Ab1!Ab1!")]
    [InlineData("C0mplex#Passw0rd")]
    public void IsValid_MeetsAllRules_ReturnsTrue(string password)
    {
        Assert.True(PasswordPolicy.IsValid(password));
    }

    [Theory]
    [InlineData("Ab1!Ab1")]
    [InlineData("password123!")]
    [InlineData("PASSWORD123!")]
    [InlineData("Password!!!!")]
    [InlineData("Password1234")]
    [InlineData("")]
    public void IsValid_MissingRule_ReturnsFalse(string password)
    {
        Assert.False(PasswordPolicy.IsValid(password));
    }
}
