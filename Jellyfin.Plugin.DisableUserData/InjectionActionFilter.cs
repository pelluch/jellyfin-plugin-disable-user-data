using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.DisableUserData;

public static class InjectActionFilter
{
    /// <summary>
    /// Attach an action filter instance to all actions that match <paramref name="matcher"/>.
    /// </summary>
    public static int AddDynamicFilter<T>(
        this IActionDescriptorCollectionProvider provider,
        IServiceProvider serviceProvider,
        Func<ControllerActionDescriptor, bool> matcher)
        where T : IFilterMetadata
    {
        var targetActions = provider.ActionDescriptors.Items
            .OfType<ControllerActionDescriptor>()
            .Where(matcher)
            .ToArray();

        foreach (var action in targetActions)
        {
            // Let DI construct the filter so constructor dependencies work.
            var filter = ActivatorUtilities.CreateInstance<T>(serviceProvider);

            action.FilterDescriptors.Add(
                new FilterDescriptor(filter, FilterScope.Global));
        }

        return targetActions.Length;
    }
}
