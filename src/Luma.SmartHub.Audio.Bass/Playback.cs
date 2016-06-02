using System;
using System.Collections.Generic;
using System.Linq;
using Luma.SmartHub.Audio.Bass.Extensions;
using Luma.SmartHub.Audio.Playback;
using ManagedBass;
using ManagedBass.Mix;

namespace Luma.SmartHub.Audio.Bass
{
    public class Playback : IPlayback, IDisposable
    {
        public string Id { get; }
        
        public bool IsPlaying { get; private set; }

        private readonly List<Channel> _outputChannels = new List<Channel>();
        private readonly Channel _sourceChannel;

        public Playback(Channel sourceChannel)
        {
            _sourceChannel = sourceChannel;
            _sourceChannel.Position = 0;
        }

        public void Play()
        {
            var channel = _outputChannels.FirstOrDefault();

            if (channel?.Start() == true)
            {
                IsPlaying = true;
            }
        }

        public void Pause()
        {
            var channel = _outputChannels.FirstOrDefault();

            if (channel?.Pause() == true)
            {
                IsPlaying = false;
            }
        }
        
        public void Stop()
        {
            var channel = _outputChannels.FirstOrDefault();

            if (channel?.Stop() == true)
            {
                IsPlaying = false;
            }
        }

        public void AddOutgoingConnection(IAudioDevice audioDevice)
        {
            var playbackDevice = audioDevice.AsPlayback();

            playbackDevice.Init();

            var splitterChannel = new SplitChannel(_sourceChannel)
            {
                Device = playbackDevice
            };

            foreach (var outputChannel in _outputChannels)
            {
                outputChannel.Link(splitterChannel.Handle);
                splitterChannel.Link(outputChannel.Handle);
            }

            _outputChannels.Add(splitterChannel);

            if (IsPlaying)
            {
                var position = _outputChannels.Min(c => c.Position);

                foreach (var outputChannel in _outputChannels)
                {
                    outputChannel.Pause();
                }

                foreach (var outputChannel in _outputChannels)
                {
                    outputChannel.Position = position;
                }

                Play();
            }
        }


        public string Write()
        {
            var result = $"Position = {_sourceChannel.Position}\n";

            foreach (var outputChannel in _outputChannels)
            {
                result += $"Position = {outputChannel.Position}\n";
            }

            return result;
        }

        public void RemoveOutgoingConnection(IAudioDevice audioDevice)
        {
            var playbackDevice = audioDevice.AsPlayback();

            var splitterChannel = _outputChannels.Single(c => c.Device == playbackDevice);

            _outputChannels.Remove(splitterChannel);

            foreach (var outputChannel in _outputChannels)
            {
                ManagedBass.Bass.ChannelRemoveLink(splitterChannel.Handle, outputChannel.Handle);
                ManagedBass.Bass.ChannelRemoveLink(outputChannel.Handle, splitterChannel.Handle);
            }

            splitterChannel.Dispose();
        }


        public void Dispose()
        {
            foreach (var outputChannel1 in _outputChannels)
            {
                foreach (var outputChannel2 in _outputChannels)
                {
                    if (outputChannel1 != outputChannel2)
                    {
                        ManagedBass.Bass.ChannelRemoveLink(outputChannel1.Handle, outputChannel2.Handle);
                    }
                }

                outputChannel1.Dispose();
            }

            _sourceChannel.Dispose();
        }
    }
}
