using Microsoft.Extensions.Configuration;

namespace TaskManager.Api.Configuration;

public class TextConfigurationSource : IConfigurationSource
{
    public string Path { get; set; } = "";

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        var fileProvider = builder.GetFileProvider();
        var fileInfo = fileProvider.GetFileInfo(Path);
        var realPath = fileInfo.PhysicalPath ?? Path;

        return new TextConfigurationProvider(realPath);
    }
}
