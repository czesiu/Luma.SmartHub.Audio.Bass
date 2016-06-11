using System;
using System.Collections.Generic;
using System.Linq;
using Luma.SmartHub.Audio.Bass.Extensions;
using Luma.SmartHub.Audio.Playback;
using ManagedBass;
using ManagedBass.Mix;
using ManagedBass.Tags;

namespace Luma.SmartHub.Audio.Bass
{
    public abstract class Playback : IPlayback, IDisposable
    {
        public string Id { get; }
        public double? Duration { get; }

        public double Volume
        {
            get { return SourceChannel.Volume; }
            set { SourceChannel.Volume = value; }
        }

        public double Position
        {
            get { return SourceChannel.Position; }
            set { SourceChannel.Position = value; }
        }

        public string Name { get; set; }

        public bool IsPlaying { get; private set; }

        ICollection<IOutputAudioDevice> OutgoingConnections { get; }

        IEnumerable<IOutputAudioDevice> IPlayback.OutgoingConnections => OutgoingConnections;

        public event EventHandler Ended;

        protected readonly List<Channel> OutputChannels = new List<Channel>();

        private Channel _sourceChannel;

        protected Channel SourceChannel
        {
            get { return _sourceChannel; }
            set
            {
                if (_sourceChannel == value)
                    return;

                _sourceChannel = value;

                OnSourceChannelChanged();
            }
        }

        public Playback()
        {
            Id = Guid.NewGuid().ToString();
            OutgoingConnections = new HashSet<IOutputAudioDevice>();
        }

        public Playback(Channel sourceChannel)
        {
            SourceChannel = sourceChannel;
        }

        private void OnSourceChannelChanged()
        {
            SourceChannel.Position = 0;
            SourceChannel.MediaEnded += OnMediaEnded;

            var networkChannel = SourceChannel as NetworkChannel;
            if (networkChannel != null)
            {
                networkChannel.DownloadComplete += () =>
                {
                    var tags = TagReader.Read(networkChannel.Handle);

                    Name = $"{tags.Artist} - {tags.Title}";
                };
            }
        }

        private void OnMediaEnded(object sender, EventArgs eventArgs)
        {
            Ended?.Invoke(sender, eventArgs);
        }

        public void Play()
        {
            var channel = OutputChannels.FirstOrDefault();

            if (channel?.Start() == true)
            {
                IsPlaying = true;
            }
        }

        public void Pause()
        {
            var channel = OutputChannels.FirstOrDefault();

            if (channel?.Pause() == true)
            {
                IsPlaying = false;
            }
        }
        
        public void Stop()
        {
            var channel = OutputChannels.FirstOrDefault();

            if (channel?.Stop() == true)
            {
                IsPlaying = false;
            }
        }

        public void AddOutgoingConnection(IOutputAudioDevice audioDevice)
        {
            var playbackDevice = audioDevice.AsPlayback();

            playbackDevice.Init();

            var splitterChannel = new SplitChannel(SourceChannel)
            {
                Device = playbackDevice
            };

            foreach (var outputChannel in OutputChannels)
            {
                outputChannel.Link(splitterChannel.Handle);
                splitterChannel.Link(outputChannel.Handle);
            }

            OutputChannels.Add(splitterChannel);
            
            OutgoingConnections.Add(audioDevice);

            if (IsPlaying)
            {
                var position = OutputChannels.Min(c => c.Position);

                foreach (var outputChannel in OutputChannels)
                {
                    outputChannel.Pause();
                }

                foreach (var outputChannel in OutputChannels)
                {
                    // Set position twice, because sometimes isn't working
                    outputChannel.Position = position;
                    outputChannel.Position = position;
                }

                Play();
            }
        }


        public string Write()
        {
            var result = $"Position = {SourceChannel.Position}\n";

            foreach (var outputChannel in OutputChannels)
            {
                result += $"Position = {outputChannel.Position}\n";
            }

            return result;
        }

        public void RemoveOutgoingConnection(IOutputAudioDevice audioDevice)
        {
            var playbackDevice = audioDevice.AsPlayback();

            var splitterChannel = OutputChannels.Single(c => c.Device == playbackDevice);

            OutputChannels.Remove(splitterChannel);

            foreach (var outputChannel in OutputChannels)
            {
                ManagedBass.Bass.ChannelRemoveLink(splitterChannel.Handle, outputChannel.Handle);
                ManagedBass.Bass.ChannelRemoveLink(outputChannel.Handle, splitterChannel.Handle);
            }

            OutgoingConnections.Remove(audioDevice);

            splitterChannel.Dispose();
        }


        public void Dispose()
        {
            foreach (var outputChannel1 in OutputChannels)
            {
                foreach (var outputChannel2 in OutputChannels)
                {
                    if (outputChannel1 != outputChannel2)
                    {
                        ManagedBass.Bass.ChannelRemoveLink(outputChannel1.Handle, outputChannel2.Handle);
                    }
                }

                outputChannel1.Dispose();
            }

            SourceChannel.MediaEnded -= OnMediaEnded;
            SourceChannel.Dispose();
        }
    }
}
