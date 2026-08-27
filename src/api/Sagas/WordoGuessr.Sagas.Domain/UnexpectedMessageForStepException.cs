namespace WordoGuessr.Sagas.Domain;

internal sealed class UnexpectedMessageForStepException : Exception
{
    public Type MessageType { get; }
    public string CurrentStep { get; }

    public UnexpectedMessageForStepException(Type messageType, string currentStep)
        : base($"Unexpected message received - {messageType} for current saga step - {currentStep}")
    {
        MessageType = messageType;
        CurrentStep = currentStep;
    }
}