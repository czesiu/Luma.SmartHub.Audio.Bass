using System;
using ManagedBass;

namespace Luma.SmartHub.Audio.Bass
{
    public class PlaybackAudioDevice : AudioDevice, IOutputAudioDevice, IDisposable
    {
        public PlaybackDevice PlaybackDevice { get; set; }

        public double Volume
        {
            get
            {
                try
                {
                    return PlaybackDevice.Volume;
                }
                catch
                {
                    return 0;
                }
            }
            set { PlaybackDevice.Volume = value; }
        }

        public override string ToString()
        {
            return $"Id = {Id}\n" +
                   $"Name = {Name}\n" +
                   $"Type = {DeviceInfo.Type}\n" +
                   $"Driver = {DeviceInfo.Driver}\n" +
                   $"IsDefault = {DeviceInfo.IsDefault}\n" +
                   $"IsEnabled = {DeviceInfo.IsEnabled}\n" +
                   $"IsInitialized = {DeviceInfo.IsInitialized}";
        }

        public void Dispose()
        {
            if (PlaybackDevice.DeviceInfo.IsInitialized)
            {
                PlaybackDevice.Dispose();
            }
        }
    }
}