using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.DisableUserData.Configuration;

public class PluginConfiguration : BasePluginConfiguration
{
    public PluginConfiguration()
    {
        Enabled = true;
    }

    /// <summary>
    /// Simple on / off toggle for the plugin
    /// </summary>
    public bool Enabled { get; }
}
