namespace HBDPG2.Core;

public class KJSDBvk
{
    public void Method()
    {
        var generator = new Generator(SpecificationVersion.V10)
        {
            Passphrase1 = "password",
            Passphrase2 = "12345678",
            PasswordLength = 32
        };

        var result = generator.GeneratePassword();
    }
}