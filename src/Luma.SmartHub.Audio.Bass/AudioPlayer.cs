using System;
using System.Collections.Generic;
using Luma.SmartHub.Audio.Playback;

namespace Luma.SmartHub.Audio.Bass
{
    public class AudioPlayer : IAudioPlayer
    {
        IEnumerable<IPlayback> IAudioPlayer.Playbacks => Playbacks;

        public IAudioHub AudioHub { get; }

        List<IPlayback> Playbacks { get; }

        public AudioPlayer(IAudioHub audioHub)
        {
            AudioHub = audioHub;
            Playbacks = new List<IPlayback>();
        }

        public void AddPlayback(IPlayback playback)
        {
            Playbacks.Add(playback);
        }

        public void RemovePlayback(IPlayback playback)
        {
            Playbacks.Remove(playback);

            var disposable = playback as IDisposable;
            disposable?.Dispose();
        }
    }
}
