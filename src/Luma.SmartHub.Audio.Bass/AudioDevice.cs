using ManagedBass;

namespace Luma.SmartHub.Audio.Bass
{
    public abstract class AudioDevice : IAudioDevice
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public DeviceInfo DeviceInfo { get; set; }
    }
}
