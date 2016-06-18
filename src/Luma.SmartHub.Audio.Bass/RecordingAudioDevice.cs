using System;
using ManagedBass;

namespace Luma.SmartHub.Audio.Bass
{
    public class RecordingAudioDevice : AudioDevice, IDisposable
    {
        public RecordingDevice RecordingDevice { get; set; }

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
            if (RecordingDevice.DeviceInfo.IsInitialized)
            {
                RecordingDevice.Dispose();
            }
        }
    }
}