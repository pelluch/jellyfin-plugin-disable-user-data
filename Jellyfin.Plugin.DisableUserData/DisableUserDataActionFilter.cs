using System;
using System.Linq;
using System.Threading.Tasks;
using Jellyfin.Plugin.DisableUserData.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Jellyfin.Plugin.DisableUserData;

public sealed class DisableUserDataActionFilter : IAsyncActionFilter
{
    private readonly ILibraryManager _libraryManager;
    private readonly ILogger<DisableUserDataActionFilter> _logger;

    public DisableUserDataActionFilter(
        ILibraryManager libraryManager,
        ILogger<DisableUserDataActionFilter> logger)
    {
        _libraryManager = libraryManager;
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var config = Plugin.Instance?.Configuration;
        if (config is null)
        {
            await next();
            return;
        }

        var request = context.HttpContext.Request;
        _logger.LogDebug("Intercepting path {Path} to see whether we disable UserData", request.Path);

        // This if is mostly for short-circuiting purposes
        if (DisabledForItems(config, context, request)
            || DisabledForCollections(config, context, request)
            || DisabledForContinueWatching(config, context, request)
            || DisabledForNextUp(config, context, request)
            || DisabledForRecentlyAdded(config, context, request))
        {
            await next();
            return;
        }

        _logger.LogDebug("DisableUserDataActionFilter not applying to path {Path}", request.Path);

        await next();
    }

    private bool DisabledForItems(
        PluginConfiguration config,
        ActionExecutingContext context,
        HttpRequest request)
    {
        if (request.Path.ToString().EndsWith("/Items", StringComparison.InvariantCultureIgnoreCase) && config.DisableOnAllItems)
        {
            DisableUserData(context);
            return true;
        }

        return false;
    }

    private bool DisabledForCollections(
        PluginConfiguration config,
        ActionExecutingContext context,
        HttpRequest request)
    {
        if (!config.DisableOnCollections)
        {
            return false;
        }

        // Handles cases where the parent is not the collections folder, but collections are included.
        // Applies for things like navigating to Wolphin's Movies, then selecting collections
        if (request.Query.TryGetValue("includeItemTypes", out StringValues includeItemTypes) &&
            includeItemTypes.Contains("BoxSet"))
        {
            DisableUserData(context);
            _logger.LogInformation("Disabling UserData for folder at path {Path}", request.Path);
            return true;
        }

        // Handles cases where the parent is the collections folder, such as navigating to collections from the home
        // on Jellyfin web, Jellyfin Media Player, and others
        if (request.Query.TryGetValue("parentId", out StringValues parentIdValues) &&
            Guid.TryParse(parentIdValues[0], out var parentId))
        {
            BaseItem? parent = _libraryManager.GetItemById(parentId);
            if (parent is CollectionFolder)
            {
                DisableUserData(context);
                _logger.LogInformation("Disabling UserData for CollectionFolder with collections at path {Path}", request.Path);
                return true;
            }
        }

        return false;
    }

    private bool DisabledForContinueWatching(
        PluginConfiguration config,
        ActionExecutingContext context,
        HttpRequest request)
    {
        if (!config.DisableOnContinueWatching)
        {
            return false;
        }

        if (request.Path.ToString().EndsWith("/Resume", StringComparison.InvariantCultureIgnoreCase))
        {
            DisableUserData(context);
            _logger.LogInformation("Disabling UserData for Continue Watching at path {Path}", request.Path);
            return true;
        }

        return false;
    }

    private bool DisabledForNextUp(
        PluginConfiguration config,
        ActionExecutingContext context,
        HttpRequest request)
    {
        if (!config.DisableOnNextUp)
        {
            return false;
        }

        if (request.Path.ToString().EndsWith("/NextUp", StringComparison.InvariantCultureIgnoreCase))
        {
            DisableUserData(context);
            _logger.LogInformation("Disabling UserData for Next Up at path {Path}", request.Path);
            return true;
        }

        return false;
    }

    private bool DisabledForRecentlyAdded(
        PluginConfiguration config,
        ActionExecutingContext context,
        HttpRequest request)
    {
        if (!config.DisableOnRecentlyAdded)
        {
            return false;
        }

        if (request.Path.ToString().EndsWith("/Latest", StringComparison.InvariantCultureIgnoreCase))
        {
            DisableUserData(context);
            _logger.LogInformation("Disabling UserData for Recently Added at path {Path}", request.Path);
            return true;
        }

        return false;
    }

    private void DisableUserData(ActionExecutingContext context)
    {
        context.ActionArguments["enableUserData"] = false;
    }
}
