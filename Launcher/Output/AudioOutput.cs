using NAudio.CoreAudioApi;
using System.Collections.Generic;

namespace Launcher.Output;

public class AudioOutput
{
    public AudioOutput(MMDevice device)
    {
        Device = device;
    }

    public readonly MMDevice Device;

    public string Name => Device.FriendlyName;
    public string DevicePath => Device.DeviceTopology.DeviceId;

    static List<AudioOutput>? cachedOutputs_;
    public static List<AudioOutput> EnumerateOutputs()
    {
        lock (typeof(AudioOutput))
        {
            if (cachedOutputs_ != null) return cachedOutputs_;

            List<AudioOutput> result = new();
            MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
            foreach (MMDevice device in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
            {
                result.Add(new AudioOutput(device));
            }
            cachedOutputs_ = result;
            return result;
        }
    }
}