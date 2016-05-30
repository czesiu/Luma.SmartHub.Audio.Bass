using ManagedBass;

namespace Luma.SmartHub.Audio.Bass
{
    public class PlaybackAudioDevice : AudioDevice
    {
        public PlaybackDevice PlaybackDevice { get; set; }

        public double Volume
        {
            get { return PlaybackDevice.Volume; }
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
    }
}