using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Luma.SmartHub.Audio.Playback;
using Luma.SmartHub.Plugins.Youtube;

namespace Luma.SmartHub.Audio.Bass.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            PlayOnAllDevices("http://czesio-w-it.2ap.pl/wp-content/uploads/2016/a.mp3");
            //PlayPlaylist("");
            Console.ReadKey();
        }

        private static void PlayPlaylist(string url)
        {
            var playbackInfoProvider = new YoutubePlaybackInfoProvider();
            var audioHub = new AudioHub(playbackInfoProvider);
            
            var allDevices = audioHub.Devices
                .OfType<PlaybackAudioDevice>()
                .ToArray();

            var playbackUris = new YoutubePlaylistProvider().CreatePlaylist(new Uri(url));
            var playback = new PlaylistPlayback(audioHub, playbackUris);

            foreach (var playbackAudioDevice in allDevices)
            {
                playback.AddOutgoingConnection(playbackAudioDevice);
            }

            playback.CurrentTrack = playback.Tracks[15];
            playback.Play();
        }

        private static void PlayOnAllDevices(string url)
        {
            var audioHub = new AudioHub(new CompositePlaybackInfoProvider(new IPlaybackInfoProvider[0]));

            var allDevices = audioHub.Devices
                .OfType<PlaybackAudioDevice>()
                .ToArray();

            var playback = audioHub.CreatePlayback(new Uri(url));

            foreach (var playbackAudioDevice in allDevices)
            {
                playback.AddOutgoingConnection(playbackAudioDevice);
            }
            
            playback.Play();

            var random = new Random();

            Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(5000);

                    var index = random.Next(allDevices.Length);

                    var device = allDevices[index];

                    playback.RemoveOutgoingConnection(device);

                    Thread.Sleep(5000);

                    playback.AddOutgoingConnection(device);
                }
            });

            while (true)
            {
                Console.Clear();
                Console.WriteLine(((Playback)playback).Write());

                Thread.Sleep(100);
            }
        }
    }
}
