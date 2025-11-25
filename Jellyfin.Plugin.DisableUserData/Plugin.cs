using System;
using System.Collections.Generic;
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
                var count = actionDescriptorProvider.AddDynamicFilter<DisableUserDataActionFilter>(
                    serviceProvider,
                    cad =>
                    {
                        var fullName = cad.ControllerTypeInfo.FullName;
                        var methodName = cad.MethodInfo.Name;

                        return string.Equals(fullName, "Jellyfin.Api.Controllers.ItemsController", StringComparison.Ordinal)
                               && (string.Equals(methodName, "GetItems", StringComparison.Ordinal)
                                   || string.Equals(methodName, "GetItemsByUserIdLegacy", StringComparison.Ordinal));
                    });

                _logger.LogInformation(
                    "Collections Accelerator: attached CollectionsActionFilter to {Count} actions",
                    count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Collections Accelerator: failed to attach action filter");
            }
        });
    }

    public override string Name => "Collections Accelerator";

    public override Guid Id => Guid.Parse("b24c5930-c337-4e0f-977f-1d900629ad09");

    public override string Description =>
        "Omits UserData (watch status) from the Collections view in order to massively speed up its loading";

    public IEnumerable<PluginPageInfo> GetPages()
    {
        return
        [
            new PluginPageInfo
            {
                Name = Name,
                EmbeddedResourcePath = string.Format(CultureInfo.InvariantCulture, "{0}.Configuration.configPage.html", GetType().Namespace)
            }
        ];
    }
}
