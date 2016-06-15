using System;
using Luma.SmartHub.Audio.Playback;
using ManagedBass;

namespace Luma.SmartHub.Audio.Bass
{
    public class UriPlayback : Playback, IUriPlayback
    {
        public Uri Uri { get; }

        public UriPlayback(IPlaybackInfoProvider playbackInfoProvider, Uri uri)
        {
            Uri = uri;

            var targetUri = uri;
            var playbackInfo = playbackInfoProvider.Get(uri);
            if (playbackInfo != null)
            {
                Name = playbackInfo.Name;
                targetUri = playbackInfo.Uri;
            }

            var url = Uri.EscapeUriString(targetUri.ToString());

            SourceChannel = new NetworkChannel(url, IsDecoder: true);
        }
    }
}
