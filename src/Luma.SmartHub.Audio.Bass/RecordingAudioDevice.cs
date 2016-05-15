using ManagedBass;

namespace Luma.SmartHub.Audio.Bass
{
    public class RecordingAudioDevice : AudioDevice
    {
        public RecordingDevice RecordingDevice { get; set; }

        public override string ToString()
        {
            return $"Id = {Id}\n" +
                   $"Name = {Name}\n" +
                   $"Type = {DeviceInfo.Type}\n" +
                   $"Driver = {DeviceInfo.Driver}";
        }
    }
}