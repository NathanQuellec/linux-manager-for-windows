using LinuxManager.Models;

namespace LinuxManager.Messages;

public class ShowDistroStopButtonMessage
{
    public Distribution Distribution { get; }

    public ShowDistroStopButtonMessage(Distribution distribution)
    {
        this.Distribution = distribution; 

    }
}