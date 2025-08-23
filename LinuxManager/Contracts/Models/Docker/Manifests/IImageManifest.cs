namespace LinuxManager.Contracts.Models.Docker.Manifests;

public interface IImageManifest
{
    public List<string> GetLayers();
}