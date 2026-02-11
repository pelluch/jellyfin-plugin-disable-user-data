using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.DisableUserData.Configuration;

public class PluginConfiguration : BasePluginConfiguration
{

    public PluginConfiguration()
    {
        DisableOnAllItems = false;
        DisableOnCollections = false;
        DisableOnNextUp = false;
        DisableOnContinueWatching = false;
        DisableOnRecentlyAdded = false;
        DisableOnSeasons = false;
        EnableRoku = true;
        DisableLogging = false;
    }

    /// <summary>
    /// Disable User Data for any Items endpoints.
    /// (GetItems + GetItemsByUserIdLegacy).
    /// </summary>
    public bool DisableOnAllItems { get; set; }

    /// <summary>
    /// Disable User Data for collections on Items endpoints.
    /// (GetItems + GetItemsByUserIdLegacy).
    /// </summary>
    public bool DisableOnCollections { get; set; }

    /// <summary>
    /// Disable User Data on Continue Watching endpoint
    /// (GetResumeItemsLegacy).
    /// </summary>
    public bool DisableOnContinueWatching { get; set; }

    /// <summary>
    /// Disable User Data on NextUp endpoint
    /// (GetNextUp).
    /// </summary>
    public bool DisableOnNextUp { get; set; }

    /// <summary>
    /// Disable User Data on Recently Added endpoints
    /// (GetResumeItemsLegacy + GetResumeItems).
    /// </summary>
    public bool DisableOnRecentlyAdded { get; set; }

    /// <summary>
    /// Disable User Data for /Shows/{id}/Seasons endpoint.
    /// </summary>
    public bool DisableOnSeasons { get; set; }

    /// <summary>
    /// Enable UserData for Roku clients.
    /// </summary>
    public bool EnableRoku { get; set; }

    /// <summary>
    /// Suppress all plugin logging output.
    /// </summary>
    public bool DisableLogging { get; set; }

    public override string ToString()
    {
         return $"{nameof(DisableOnAllItems)}: {DisableOnAllItems}, " +
             $"{nameof(DisableOnCollections)}: {DisableOnCollections}, " +
             $"{nameof(DisableOnContinueWatching)}: {DisableOnContinueWatching}, " +
             $"{nameof(DisableOnNextUp)}: {DisableOnNextUp}, " +
             $"{nameof(DisableOnRecentlyAdded)}: {DisableOnRecentlyAdded}, " +
             $"{nameof(DisableOnSeasons)}: {DisableOnSeasons}, " +
             $"{nameof(EnableRoku)}: {EnableRoku}" +
             $"{nameof(DisableLogging)}: {DisableLogging}, ";
    }
}
