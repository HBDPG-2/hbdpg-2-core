namespace HBDPG2.Core.Tests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var generator = new Generator(SpecificationVersion.V10)
        {
            Passphrase1 = "password".ToCharArray(),
            Passphrase2 = "12345678".ToCharArray(),
            PasswordLength = 32
        };

        var result = generator.GeneratePassword();

        Assert.NotNull(result.Password);
        Assert.Equal("D#Z2c8h1Xbl*70<6eV1m45X\"\\r5d7T.T", new string(result.Password));
    }
}
