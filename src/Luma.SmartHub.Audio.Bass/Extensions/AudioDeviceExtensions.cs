using System;
using ManagedBass;

namespace Luma.SmartHub.Audio.Bass.Extensions
{
    public static class AudioDeviceExtensions
    {
        public static IAudioDevice ToAudioDevice(this PlaybackDevice playbackDevice)
        {
            return new PlaybackAudioDevice
            {
                Id = $"{AudioDevices.Output}-{playbackDevice.DeviceIndex}",
                Type = AudioDevices.Output,
                DeviceInfo = playbackDevice.DeviceInfo,
                PlaybackDevice = playbackDevice
            };
        }

        public static IAudioDevice ToAudioDevice(this RecordingDevice recordingDevice)
        {
            return new RecordingAudioDevice
            {
                Id = $"{AudioDevices.Input}-{recordingDevice.DeviceIndex}",
                Name = recordingDevice.DeviceInfo.Name,
                Type = AudioDevices.Input,
                DeviceInfo = recordingDevice.DeviceInfo,
                RecordingDevice = recordingDevice
            };
        }

        public static PlaybackDevice AsPlayback(this IAudioDevice audioDevice)
        {
            var playbackAudioDevice = audioDevice as PlaybackAudioDevice;
            if (playbackAudioDevice == null)
                throw new ArgumentException("Invalid type - should be PlaybackAudioDevice");

            return playbackAudioDevice.PlaybackDevice;
        }

        public static RecordingDevice AsRecording(this IAudioDevice audioDevice)
        {
            var recordingAudioDevice = audioDevice as RecordingAudioDevice;
            if (recordingAudioDevice == null)
                throw new ArgumentException("Invalid type - should be RecordingAudioDevice");

            return recordingAudioDevice.RecordingDevice;
        }
    }
}
