﻿using System;
using System.Collections.Generic;
using System.Linq;
using ManagedBass;
using Luma.SmartHub.Audio.Bass.Extensions;
using Luma.SmartHub.Audio.Playback;
using ManagedBass.Mix;

namespace Luma.SmartHub.Audio.Bass
{
    public class AudioHub : IAudioHub
    {
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

        public IPlayback CreatePlayback(Uri uri)
        {
            return new Playback(new NetworkChannel(uri.ToString(), IsDecoder: true));
        }

        public void Play(string url, IAudioDevice device = null)
        {
            var playbackDevice = device?.AsPlayback();

            playbackDevice?.Init();

            var player = new NetworkChannel(url);

            if (playbackDevice != null)
            {
                player.Device = playbackDevice;
            }

            player.Start();
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
    }
}
