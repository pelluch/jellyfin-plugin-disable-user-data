using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using Jellyfin.Plugin.DisableUserData.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.DisableUserData;

public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    private readonly ILogger<Plugin> _logger;

    public static Plugin? Instance { get; private set; }


    public Plugin(
        IApplicationPaths applicationPaths,
        IXmlSerializer xmlSerializer,
        ILogger<Plugin> logger,
        IServiceProvider serviceProvider,
        IActionDescriptorCollectionProvider actionDescriptorProvider,
        IHostApplicationLifetime hostApplicationLifetime)
        : base(applicationPaths, xmlSerializer)
    {
        _logger = logger;
        Instance = this;

        // Wait until the app is fully started so all action descriptors exist.
        hostApplicationLifetime.ApplicationStarted.Register(() =>
        {
            try
            {
                const string BaseName = "Jellyfin.Api.Controllers";
                var controllersMap = new Dictionary<string, ImmutableList<string>>
                {
                    [$"{BaseName}.ItemsController"] = ImmutableList.Create("GetItemsByUserIdLegacy", "GetItems", "GetResumeItemsLegacy", "GetResumeItems"),
                    [$"{BaseName}.UserLibraryController"] = ImmutableList.Create("GetLatestMediaLegacy", "GetLatestMedia"),
                    [$"{BaseName}.TvShowsController"] = ImmutableList.Create("GetNextUp", "GetSeasons")
                };
                var count = actionDescriptorProvider.AddDynamicFilter<DisableUserDataActionFilter>(
                    serviceProvider,
                    cad =>
                    {
                        var controllerName = cad.ControllerTypeInfo.FullName;
                        var methodName = cad.MethodInfo.Name;

                        if (controllerName == null)
                        {
                            return false;
                        }

                        return controllersMap.TryGetValue(controllerName, out var methodNames)
                               && methodNames.Contains(methodName);
                    });

                _logger.LogInformation("Attached DisableUserDataActionFilter to {Count} actions", count);
                _logger.LogInformation("Plugin configuration: {Configuration}", Configuration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to attach action filter");
            }
        });
    }

    public override string Name => "Disable User Data";

    public override Guid Id => Guid.Parse("b24c5930-c337-4e0f-977f-1d900629ad09");

    public override string Description =>
        "Omits UserData (watch status) from different endpoints in order to speed up queries";

    public IEnumerable<PluginPageInfo> GetPages()
    {
        return
        [
            new PluginPageInfo { Name = Name, EmbeddedResourcePath = string.Format(CultureInfo.InvariantCulture, "{0}.Configuration.configPage.html", GetType().Namespace) }
        ];
    }
}
