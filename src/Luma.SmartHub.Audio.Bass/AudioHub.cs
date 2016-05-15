using System.Collections.Generic;
using System.Linq;
using ManagedBass;
using Luma.SmartHub.Audio.Bass.Extensions;

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
    }
}
