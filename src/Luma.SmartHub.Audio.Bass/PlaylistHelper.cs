using System;
using System.Collections.Generic;
using System.Linq;
using Luma.SmartHub.Audio.Playback;

namespace Luma.SmartHub.Audio.Bass
{
    public static class PlaylistHelper
    {
        public static IList<IPlayback> ToPlaybackList(this IList<Uri> tracks, IPlaybackManager playbackManager)
        {
            return tracks
                .Select(c => new UriPlayback(playbackManager, c))
                .Cast<IPlayback>()
                .ToList();
        }
    }
}