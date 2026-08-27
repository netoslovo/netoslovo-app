namespace WordoGuessr.Auth.App.Abstractions;

public interface IOutbox
{
    ValueTask Send<T>(T message);
}