namespace WordoGuessr.Auth.App.Services;

public interface IOtpCodeGenerator
{
    string Generate(int codeLength);
}
