using System;
using System.Collections.Generic;
using System.Linq;
using Luma.SmartHub.Audio.Playback;
using ManagedBass;

namespace Luma.SmartHub.Audio.Bass
{
    public static class PlaylistHelper
    {
        public static IList<IPlayback> ToPlaybackList(this IList<Uri> tracks)
        {
            return tracks
                .Select(c => new Playback(new NetworkChannel(Uri.EscapeUriString(c.ToString()))))
                .Cast<IPlayback>()
                .ToList();
        }
    }
}