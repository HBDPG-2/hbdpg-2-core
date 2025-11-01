namespace HBDPG2.Core.Tests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var generator = new Generator(SpecificationVersion.V10)
        {
            Passphrase1 = "password",
            Passphrase2 = "12345678",
            PasswordLength = 32
        };

        var result = generator.GeneratePassword();

        Assert.NotNull(result.Password);
        Assert.Equal("D#Z2c8h1Xbl*70<6eV1m45X\"\\r5d7T.T", result.Password);
    }
}
