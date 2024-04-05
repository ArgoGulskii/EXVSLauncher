using Avalonia.Media.Imaging;
using Launcher.Views;
using NAudio.Wave;
using ReactiveUI;
using System.Collections.Generic;
using NAudio.CoreAudioApi;
using System.Reflection;

namespace Launcher.Output;

public class OutputAssignment : ReactiveObject
{
    public OutputAssignment()
    {
    }

    private bool isFirst_;
    public bool IsFirst
    {
        get => isFirst_;
        set
        {
            this.RaiseAndSetIfChanged(ref isFirst_, value);
        }
    }

    private bool isLast_;
    public bool IsLast
    {
        get => isLast_;
        set
        {
            this.RaiseAndSetIfChanged(ref isLast_, value);
        }
    }

    public int AudioIndex { get; set; }
    public int DisplayIndex { get; set; }

    public static List<DisplayOutput> DisplayOutputs => DisplayOutput.EnumerateDisplays();
    public static List<AudioOutput> AudioOutputs => AudioOutput.EnumerateOutputs();

    public DisplayOutput Display => DisplayOutputs[DisplayIndex];
    public AudioOutput Audio => AudioOutputs[AudioIndex];

    public string DisplayName => $"Display: {Display.DropDownName}";
    public string AudioName => $"Audio: {Audio.Name}";

    PreviewWindow? Preview;
    WasapiOut? AudioPreview;
    public Bitmap? HeaderBitmap => HeaderImage.Get();

    public void StartPreview()
    {
        StopPreview();

        Preview = new PreviewWindow();
        Preview.DataContext = this;
        Preview.Show();
        Display.MoveWindow(Preview, true);

        var assembly = Assembly.GetExecutingAssembly();
        var asset = assembly.GetManifestResourceStream("Launcher.Assets.newtype.wav");

        var audioReader = new WaveFileReader(asset);
        var audioStream = new LoopStream(WaveFormatConversionStream.CreatePcmStream(audioReader));

        WasapiOut audioOut = new WasapiOut(Audio.Device, AudioClientShareMode.Shared, false, 0);
        audioOut.Init(audioStream);
        audioOut.Play();
        AudioPreview = audioOut;
    }

    public void StopPreview()
    {
        if (Preview != null)
        {
            Preview.Close();
            Preview = null;
        }

        if (AudioPreview != null)
        {
            AudioPreview.Stop();
            AudioPreview.Dispose();
            AudioPreview = null;
        }
    }
}