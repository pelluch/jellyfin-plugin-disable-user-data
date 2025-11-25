# Disable UserData Plugin

## Introduction
For many people, versions 10.11.x (latest one being 10.11.3 as of this writing) results in a very slow loading of collections, and sometimes for home items. The issue is discussed in the upstream issues:

- https://github.com/jellyfin/jellyfin/issues/15090
- https://github.com/jellyfin/jellyfin/issues/15063

And has made the app much less usable for some.

While trying to debug the issue, I noticed that a large portion of the slowdown comes from many nested queries used to get the associated `UserData` for invidual titles, which is mostly used to get watch status / progress. Its JSON looks like this:

```
  "UserData": {
    "UnplayedItemCount": 880,
    "PlaybackPositionTicks": 0,
    "PlayCount": 0,
    "IsFavorite": false,
    "Played": false,
    "Key": "ef8c5f5b-26c8-814b-bfae-3e4499649c2a",
    "ItemId": "ef8c5f5b26c8814bbfae3e4499649c2a"
  }
  ```
  Many APIs support directly passing in an `enableUserData` parameter. When this is set to `false`, slowdown caused by the above issues is significantly mitigated, at the cost of losing Unwatched badges and / or progress percentage in the UI, which in some cases may not matter.

## What the plugin does

Because the performance issues may take a while to debug, I was motivated to see if I could create a mitigation so users can actually use their libraries without major slowdowns while the underlying issues are being worked on - particularly, force `enableUserData=false` to be set in certain places on the backend, independent of what clients are being used.

I was familiar with the [jellyfin-plugin-meilisearch](https://github.com/arnesacnussem/jellyfin-plugin-meilisearch) plugin, which essentially "Hijacks" certain routes involved with search to use the Meilisearch mechanism instead, providing many improvements. In this plugin, I use a similar mechanism to capture certain routes, and depending on the plugin settings, insert the `enableUserData=false` parameter. There are no other alterations made, as after inserting the parameter the flow goes to the usual Jellyfin methods. The specific routes involved (all of which can be individually enabled or disabled in the plugin settings) are:

1. `/Items` and `/Users/{user_id}/Items`, handled by `GetItems` and `GetItemsByUserIdLegacy`, respectively. These routes are used when loading the collections view, one of the biggest painpoints with users that have large collections. Checks are made to make sure we're only doing this when the parent item is the Collections Folder (Home -> Clicking on Collections on most clients), or when filtering by `BoxSets` (Home -> Movies -> Collections in clients like Wholphin). Disabling user data here is not very impactful for the UI (you lose the "unplayed" badge item and filter in collections), particularly for users who use large collections for organizing / filtering, common with the [Auto Collections plugin](https://github.com/KeksBombe/jellyfin-plugin-auto-collections), which generates collections based on user criteria, often generating very large collections.
2. `/Items/Resume` and `/Users/{user_id}/Items/Resume`, handled by `GetResumeItems` or `GetResumeItemsLegacy`, respectively. This is essentially "Next Up". I found this less of a critical speedup, but it's still left as an option. Disabling User Data here means you don't see the track bar progress in the Home.
3. `/NextUp`, handled by `GetNextUp`. Similar to the above: This is essentially "Continue Watching", and also found it less of a critical speedup. As far as I can tell, I haven't seen UI consequences of disabling user data here, since things in "NextUp" are things you haven't watched yet, but its API still takes in this value so it's left as an option.
4. `/Items/Latest`, and `/Users/{user_id}/Items/Latest`, handled by `GetLatestMedia` and `GetLatestMediaLegacy`, respectively. This is "Recently Added". Again, disabling user data here essentially means you don't see watch progress for the media you've watched that's in those sections.

What this plugin does **not** do: Modify any data or metadata in any way at all, or incur in any destructive behavior. It simply leverages an existing parameter.

## Disclaimers ##

1. The plugin applies to all clients on all platforms. I've tested many and nothing seems to break, but you are encouraged to do your own testing as well.
2. Because of the way this plugin works (and same applies for the meilisearch one I mentioned earlier), it relies on Jellyfin not suddenly changing controller / method names for the above. This should be very rare, but there's no guarantee of this plugin working through versions (though the way it's implemented, in the worst case the parameter just won't get added).
3. Finally, this is a **mitigation**. Ideally this plugin shouldn't have to exist. As soon as the issue is fixed upstream (and I want to help look into that), this plugin should be removed.

## Compatibility

This is only for Jellyfin versions 10.11.0 and above.

Because it's server-wide, this is expected to work across clients. This has been concretely tested on Jellyfin Web (browser), Jellyfin Media Player (desktop), official Jellyfin client for Android TV, Wholphin (also for Android TV), Jellyfin on Android and Findroid on Android. I haven't been able to test on other platforms, but these APIs are common and I don't expect there to be much of an issue.

## Installation

1. Add a new repository to Jellyfin with the following URL:

    ```
    https://raw.githubusercontent.com/pelluch/jellyfin-plugins/refs/heads/main/manifest.json
    ```

    And any name you like (e.g. pelluch Plugins).
2. Reload the plugins page, and select the "Disable User Data" plugins from the list of available ones
3. Restart the server (note you need to configure it before it does anything)

## Configuration

Configuration is simple - just go to the plugin's setting page and enable what you want. By default, nothing will be enabled, you will have to manually choose. I recommending experimenting when you do. Changing the settings does not require a server restart.