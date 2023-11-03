using Launcher.ViewModels;
using Linearstar.Windows.RawInput;
using Linearstar.Windows.RawInput.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Printing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace Launcher.Input;

public readonly record struct DevicePath(string Path);

internal class InputManager
{
    public static InputManager Instance = new();

    public Dictionary<RawInputDeviceHandle, InputDevice> CurrentDevices = [];
    public Dictionary<RawInputDeviceHandle, RebindViewModel> AssignedRebinds = [];
    public List<RebindViewModel> WaitingRebinds = [];

    private readonly Window inputWindow;
    private readonly nint inputWindowHwnd;
    private readonly HwndSource inputHwndSource;

    private InputManager()
    {
        inputWindow = new Window();
        var windowInteropHelper = new WindowInteropHelper(inputWindow);
        inputWindowHwnd = windowInteropHelper.EnsureHandle();
        inputHwndSource = HwndSource.FromHwnd(inputWindowHwnd);
    }

    public void Start()
    {
        RawInputDevice.RegisterDevice(HidUsageAndPage.GamePad, RawInputDeviceFlags.InputSink | RawInputDeviceFlags.DevNotify, inputWindowHwnd);
        RawInputDevice.RegisterDevice(HidUsageAndPage.Joystick, RawInputDeviceFlags.InputSink | RawInputDeviceFlags.DevNotify, inputWindowHwnd);
        inputHwndSource.AddHook(Hook);
    }

    public void AddRebindWindow(RebindViewModel viewModel)
    {
        WaitingRebinds.Add(viewModel);
        PromptNextRebind();
    }

    public void RemoveRebindWindow(RebindViewModel viewModel)
    {
        WaitingRebinds.Remove(viewModel);
        var remove = new List<RawInputDeviceHandle>();
        foreach (var (deviceHandle, rvm) in AssignedRebinds)
        {
            if (rvm == viewModel) remove.Add(deviceHandle);
        }

        foreach (var deviceHandle in remove)
        {
            AssignedRebinds.Remove(deviceHandle);
        }
    }

    void DeviceArrived(RawInputDeviceHandle handle, RawInputDevice device)
    {
        CurrentDevices.Add(handle, new InputDevice(device));
    }

    void DeviceLeft(RawInputDeviceHandle handle)
    {
        Console.WriteLine($"Lost device: {CurrentDevices[handle]}");
        CurrentDevices.Remove(handle);

        if (AssignedRebinds.ContainsKey(handle))
        {
            var rebindState = AssignedRebinds[handle];
            rebindState.DeviceLeft();

            AssignedRebinds.Remove(handle);
            WaitingRebinds.Add(rebindState);
            PromptNextRebind();
        }
    }

    void HandleInput(RawInputHidData data)
    {
        var deviceHandle = data.Header.DeviceHandle;

        if (!CurrentDevices.ContainsKey(deviceHandle)) return;
        var device = CurrentDevices[deviceHandle];

        var inputState = InputDevice.Parse(data);

        if (AssignedRebinds.ContainsKey(deviceHandle))
        {
            AssignedRebinds[deviceHandle].HandleInput(inputState);
            return;
        }

        // Device is dangling, look for a waiting rebind window.
        if (WaitingRebinds.Count == 0) return;

        // TODO: Require a button to be held for a duration instead of accepting the first input.
        if (inputState.Buttons.IsEmpty()) return;

        var rebind = WaitingRebinds[0];
        WaitingRebinds.RemoveAt(0);
        AssignedRebinds[deviceHandle] = rebind;
        rebind.DeviceSelected(device);

        PromptNextRebind();
    }

    void PromptNextRebind()
    {
        if (WaitingRebinds.Count > 0)
        {
            WaitingRebinds[0].DeviceAvailable();
        }
    }

    private IntPtr Hook(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
    {
        const int WM_INPUT = 0x00FF;
        const int WM_INPUT_DEVICE_CHANGE = 0x00FE;
        if (msg == WM_INPUT)
        {
            // TODO: Keyboard support?
            var data = RawInputData.FromHandle(lparam) as RawInputHidData;
            if (data == null) return 0;

            HandleInput(data);
        }
        else if (msg == WM_INPUT_DEVICE_CHANGE)
        {
            const int GIDC_ARRIVAL = 1;
            const int GIDC_REMOVAL = 2;
            var handle = (RawInputDeviceHandle)lparam;
            try
            {
                var device = RawInputDevice.FromHandle(handle);

                switch (wparam)
                {
                    case GIDC_ARRIVAL:
                        DeviceArrived(handle, device);
                        break;
                    case GIDC_REMOVAL:
                        DeviceLeft(handle);
                        break;
                    default:
                        Console.WriteLine($"Unexpected WM_INPUT_DEVICE_CHANGE wparam = {wparam}");
                        break;
                }
            }
            catch (Win32ErrorException)
            {
                Console.WriteLine($"Exception occurred for device {CurrentDevices[handle]}");
                DeviceLeft(handle);
            }
        }

        return 0;
    }
}