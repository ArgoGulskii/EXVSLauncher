using Linearstar.Windows.RawInput;

namespace Launcher.Input;

enum POVDirection
{
    North,
    NorthEast,
    East,
    SouthEast,
    South,
    SouthWest,
    West,
    NorthWest,
    Neutral,
}

static class POVDirectionExt
{
    public static Direction X(this POVDirection pov)
    {
        return pov switch
        {
            POVDirection.NorthWest or POVDirection.West or POVDirection.SouthWest => Direction.Negative,
            POVDirection.NorthEast or POVDirection.East or POVDirection.SouthEast => Direction.Positive,
            _ => Direction.Neutral,
        };
    }

    public static Direction Y(this POVDirection pov)
    {
        return pov switch
        {
            POVDirection.NorthWest or POVDirection.North or POVDirection.NorthEast => Direction.Negative,
            POVDirection.SouthWest or POVDirection.South or POVDirection.SouthEast => Direction.Positive,
            _ => Direction.Neutral,
        };
    }
}

public class InputDevice
{
    public readonly string ManufacturerName;
    public readonly string ProductName;
    public readonly int VendorId;
    public readonly int ProductId;
    public readonly string DevicePath;

    public InputDevice(RawInputDevice device)
    {
        ManufacturerName = device.ManufacturerName ?? "";
        ProductName = device.ProductName ?? "";
        VendorId = device.VendorId;
        ProductId = device.ProductId;
        DevicePath = device.DevicePath!;
    }

    public override string ToString()
    {
        return $"{ManufacturerName} {ProductName}: ({VendorId:x4}:{ProductId:x4})";
    }

    public static double Scale(int min, int max, int value)
    {
        int total = max - min;
        int diff = value - min;
        return ((double)diff) / total;
    }

    public static Direction ScaleDirection(HidValue hidValue, int value)
    {
        var min = hidValue.MinValue;
        var max = hidValue.MaxValue;

        // Some controllers (e.g. Xbox Elite Series 2) specify -1 as their max value to mean the maximum value.
        // TODO: Figure out the value size instead of assuming it's 16 bit.
        if (min >= 0 && max < min) max = 65535;

        double x = Scale(min, max, value);
        if (x < 0.25) return Direction.Negative;
        if (x > 0.75) return Direction.Positive;
        return Direction.Neutral;
    }

    public static InputState Parse(RawInputHidData data)
    {
        Direction x = Direction.Neutral;
        Direction y = Direction.Neutral;
        POVDirection pov = POVDirection.Neutral;

        BitSet buttons = [];
        for (int i = 0; i < data.ButtonSetStates.Length; i++)
        {
            const int HID_USAGE_PAGE_BUTTON = 0x09;
            if (data.ButtonSetStates[i].ButtonSet.UsagePage != HID_USAGE_PAGE_BUTTON) continue;

            foreach (ushort button in data.ButtonSetStates[i].ActiveUsages)
            {
                buttons[button] = true;
            }
            break;
        }

        for (int i = 0; i < data.ValueSetStates.Length; i++)
        {
            var valueSetState = data.ValueSetStates[i];
            var valueSet = valueSetState.ValueSet;
            if (valueSet.UsagePage != 1) continue;

            foreach (var value in valueSetState)
            {
                var usage = value.Value.UsageAndPage;

                switch (usage.Usage)
                {
                    case 0x30:
                        x = ScaleDirection(value.Value, value.CurrentValue);
                        break;

                    case 0x31:
                        y = ScaleDirection(value.Value, value.CurrentValue);
                        break;

                    case 0x39:
                        int min = value.Value.MinValue;
                        pov = (value.CurrentValue - min) switch
                        {
                            0 => POVDirection.North,
                            1 => POVDirection.NorthEast,
                            2 => POVDirection.East,
                            3 => POVDirection.SouthEast,
                            4 => POVDirection.South,
                            5 => POVDirection.SouthWest,
                            6 => POVDirection.West,
                            7 => POVDirection.NorthWest,
                            _ => POVDirection.Neutral,
                        };
                        break;

                    default:
                        break;
                }
            }
        }

        if (pov != POVDirection.Neutral)
        {
            x = pov.X();
            y = pov.Y();
        }

        InputState state = new()
        {
            X = x,
            Y = y,
            Buttons = buttons,
        };

        return state;
    }
}