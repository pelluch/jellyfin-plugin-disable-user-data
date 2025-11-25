using System;
using System.Linq;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;

namespace Jellyfin.Plugin.DisableUserData;

public sealed class DisableUserDataActionFilter : IAsyncActionFilter
{
    private readonly ILibraryManager _libraryManager;

    public DisableUserDataActionFilter(ILibraryManager libraryManager)
    {
        _libraryManager = libraryManager;
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var plugin = Plugin.Instance;
        if (plugin is null || !plugin.Configuration.Enabled)
        {
            await next();
            return;
        }

        var request = context.HttpContext.Request;

        // Handles cases where the parent is not the collections folder, but collections are included.
        // Applies for things like navigating to Wolphin's Movies, then selecting collections
        if (request.Query.TryGetValue("includeItemTypes", out StringValues includeItemTypes) &&
            includeItemTypes.Contains("BoxSet"))
        {
            context.ActionArguments["enableUserData"] = false;
        }

        // Handles cases where the parent is the collections folder, such as navigating to collections from the home
        // on Jellyfin web, Jellyfin Media Player, and others
        if (request.Query.TryGetValue("parentId", out StringValues parentIdValues) &&
            Guid.TryParse(parentIdValues[0], out var parentId))
        {
            BaseItem? parent = _libraryManager.GetItemById(parentId);
            if (parent is CollectionFolder)
            {
                context.ActionArguments["enableUserData"] = false;
            }
        }

        await next();
    }
}
