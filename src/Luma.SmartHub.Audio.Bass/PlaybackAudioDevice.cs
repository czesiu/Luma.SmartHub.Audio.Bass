using ManagedBass;

namespace Luma.SmartHub.Audio.Bass
{
    public class PlaybackAudioDevice : AudioDevice
    {
        public PlaybackDevice PlaybackDevice { get; set; }

        public override string ToString()
        {
            return $"Id = {Id}\n" +
                   $"Name = {Name}\n" +
                   $"Type = {DeviceInfo.Type}\n" +
                   $"Driver = {DeviceInfo.Driver}";
        }
    }
}