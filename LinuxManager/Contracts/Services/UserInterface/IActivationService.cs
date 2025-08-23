namespace LinuxManager.Contracts.Services.UserInterface;

public interface IActivationService
{
    Task ActivateAsync(object activationArgs);
}
