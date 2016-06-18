using System;
using System.Collections.Generic;
using System.Linq;
using ManagedBass;
using Luma.SmartHub.Audio.Bass.Extensions;
using Luma.SmartHub.Audio.Playback;
using Serilog;

namespace Luma.SmartHub.Audio.Bass
{
    public class AudioHub : IAudioHub, IDisposable
    {
        protected readonly ILogger Logger = Log.ForContext<AudioHub>();

        private readonly IPlaybackInfoProvider _playbackInfoProvider;
        
        private readonly List<Playback> _playbacks = new List<Playback>();

        private List<IAudioDevice> _devices;
        public IList<IAudioDevice> Devices
        {
            get
            {
                if (_devices == null)
                {
                    _devices = new List<IAudioDevice>();

                    var playbackDevices = PlaybackDevice.Devices
                        .Where(c => c.DeviceInfo.IsEnabled)
                        .Where(c => c != PlaybackDevice.NoSoundDevice)
                        .Select(AudioDeviceExtensions.ToAudioDevice);

                    var recordingDevices = RecordingDevice.Devices
                        .Where(c => c.DeviceInfo.IsEnabled)
                        .Select(AudioDeviceExtensions.ToAudioDevice);

                    _devices.AddRange(playbackDevices);
                    _devices.AddRange(recordingDevices);
                }

                return _devices;
            }
        }

        public AudioHub(IPlaybackInfoProvider playbackInfoProvider)
        {
            _playbackInfoProvider = playbackInfoProvider;
        }

        public IUriPlayback CreatePlayback(Uri uri)
        {
            var playback = new UriPlayback(_playbackInfoProvider, uri);

            playback.Disposed += OnPlaybackDisposed;

            _playbacks.Add(playback);

            return playback;
        }

        private double? _volume;
        public double Volume
        {
            get
            {
                if (_volume == null)
                {
                    _volume = UnifyDevicesVolume();
                }

                return _volume.Value;
            }
            set
            {
                _volume = value;

                SetDevicesVolume(value);
            }
        }

        private double UnifyDevicesVolume()
        {
            var playbackDevices = Devices
                .OfType<PlaybackAudioDevice>()
                .ToArray();

            var volume = playbackDevices.Average(c => c.Volume);

            foreach (var playbackAudioDevice in playbackDevices)
            {
                playbackAudioDevice.Volume = volume;
            }

            return volume;
        }

        private void SetDevicesVolume(double volume)
        {
            var playbackDevices = Devices
                .OfType<PlaybackAudioDevice>()
                .ToArray();

            foreach (var playbackAudioDevice in playbackDevices)
            {
                playbackAudioDevice.Volume = volume;
            }
        }
        
        private void OnPlaybackDisposed(object sender, EventArgs eventArgs)
        {
            var playback = (Playback) sender;

            playback.Disposed -= OnPlaybackDisposed;

            _playbacks.Remove(playback);
        }

        public void Dispose()
        {
            Logger.Debug("Disposing audio hub");

            foreach (var playback in _playbacks)
            {
                playback.Disposed -= OnPlaybackDisposed;

                playback.Dispose();
            }

            Logger.Debug("Disposed {count} playbacks", _playbacks.Count);

            _playbacks.Clear();

            foreach (var audioDevice in Devices)
            {
                ((IDisposable)audioDevice).Dispose();
            }

            Logger.Debug("Disposed {count} audio devices", _devices.Count);

            _devices.Clear();
        }
    }
}
