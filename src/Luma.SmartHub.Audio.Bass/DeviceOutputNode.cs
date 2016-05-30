using Luma.SmartHub.Audio.Bass.Extensions;
using ManagedBass;
using ManagedBass.Mix;

namespace Luma.SmartHub.Audio.Bass
{
    public class DeviceOutputNode
    {
        public DeviceOutputNode(IAudioDevice device)
        {
            var playbackDevice = device.AsPlayback();
            var mixerStream = new MixerStream(new PCMFormat());
        }
    }
}
