using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Luma.SmartHub.Audio.Bass.Extensions;
using Luma.SmartHub.Audio.Playback;
using ManagedBass;
using ManagedBass.Mix;
using ManagedBass.Tags;
using Serilog;

namespace Luma.SmartHub.Audio.Bass
{
    public abstract class Playback : IPlayback, IDisposable
    {
        protected readonly ILogger Logger = Log.ForContext<Playback>();

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

        public event EventHandler Disposed;

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

        private void OnSourceChannelChanged()
        {
            SourceChannel.Position = 0;
            SourceChannel.MediaEnded += OnMediaEnded;

            var networkChannel = SourceChannel as NetworkChannel;
            if (networkChannel != null)
            {
                networkChannel.DownloadComplete += OnDownloadComplete;
            }
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

            if (splitterChannel.Device != playbackDevice)
            {
                splitterChannel.Dispose();

                throw new InvalidOperationException($"Cannot set outgoing connection for device {audioDevice}");
            }

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

                Logger.Debug("AddOutgoingConnection: MinPosition = {position}", position);

                foreach (var outputChannel in OutputChannels)
                {
                    outputChannel.Pause();

                    Logger.Debug("AddOutgoingConnection: After pause for outputChannel device {device} position = {position}", outputChannel.Device, outputChannel.Position);
                }

                foreach (var outputChannel in OutputChannels)
                {
                    Logger.Debug("AddOutgoingConnection: Before updating position for outputChannel device {device} position = {position}", outputChannel.Device, outputChannel.Position);

                    // Set position few times, because sometimes once isn't working
                    var i = 5;
                    while (i-- > 0)
                    {
                        outputChannel.Position = position;
                    }

                    Logger.Debug("AddOutgoingConnection: After updating position for outputChannel device {device} position = {position}", outputChannel.Device, outputChannel.Position);
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
            Stop();
            
            foreach (var outputChannel1 in OutputChannels.ToArray())
            {
                foreach (var outputChannel2 in OutputChannels)
                {
                    if (outputChannel1 != outputChannel2)
                    {
                        ManagedBass.Bass.ChannelRemoveLink(outputChannel1.Handle, outputChannel2.Handle);
                    }
                }

                OutputChannels.Remove(outputChannel1);

                outputChannel1.Dispose();
            }

            OutgoingConnections.Clear();

            var networkChannel = SourceChannel as NetworkChannel;
            if (networkChannel != null)
            {
                networkChannel.DownloadComplete -= OnDownloadComplete;
            }

            SourceChannel.MediaEnded -= OnMediaEnded;
            SourceChannel.Dispose();

            Disposed?.Invoke(this, EventArgs.Empty);
        }

        private void OnDownloadComplete()
        {
            var tags = TagReader.Read(SourceChannel.Handle);

            Name = $"{tags.Artist} - {tags.Title}";
        }

        private void OnMediaEnded(object sender, EventArgs eventArgs)
        {
            if (Ended == null)
                return;

            Task.Factory.StartNew(() => Ended?.Invoke(this, EventArgs.Empty));
        }
    }
}
