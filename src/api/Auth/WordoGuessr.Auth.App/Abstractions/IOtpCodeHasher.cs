namespace WordoGuessr.Auth.App.Abstractions;

public interface IOtpCodeHasher
{
    string Hash(string code);

    bool Verify(string code, string hash);
}
