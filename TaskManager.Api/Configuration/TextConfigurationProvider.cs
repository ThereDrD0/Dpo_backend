using Microsoft.Extensions.Configuration;

namespace TaskManager.Api.Configuration;

public class TextConfigurationProvider : ConfigurationProvider
{
    private readonly string _path;

    public TextConfigurationProvider(string path)
    {
        _path = path;
    }

    public override void Load()
    {
        if (!File.Exists(_path))
        {
            return;
        }

        var lines = File.ReadAllLines(_path)
            .Select(x => x.Trim())
            .Where(x => x.Length > 0)
            .ToArray();

        for (var i = 0; i + 1 < lines.Length; i += 2)
        {
            Data[lines[i]] = lines[i + 1];
        }
    }
}
