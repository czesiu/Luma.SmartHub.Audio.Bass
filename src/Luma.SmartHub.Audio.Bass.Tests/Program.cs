using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Luma.SmartHub.Audio.Playback;

namespace Luma.SmartHub.Audio.Bass.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            PlayOnAllDevices("http://czesio-w-it.2ap.pl/wp-content/uploads/2016/a.mp3");
            Console.ReadKey();
        }

        private static void PlayOnAllDevices(string url)
        {
            var playbackManager = new PlaybackManager(new IPlaybackInfoProvider[0]);
            var audioHub = new AudioHub(playbackManager);

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
