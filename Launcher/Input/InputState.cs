using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher.Input;

public enum Direction
{
    Neutral,
    Positive,
    Negative,
}

public struct InputState
{
    public Direction X;
    public Direction Y;
    public BitSet Buttons;

    public readonly InputDiff Diff(InputState newState)
    {
        InputDiff result = new();
        if (X == Direction.Neutral) result.X = newState.X;
        if (Y == Direction.Neutral) result.Y = newState.Y;
        result.Pressed = Buttons.Diff(newState.Buttons);
        result.Released = newState.Buttons.Diff(Buttons);
        return result;
    }

    public override readonly string ToString()
    {
        return $"X = {X}, Y = {Y}, Buttons = {Buttons}";
    }
}

public struct InputDiff
{
    public Direction X;
    public Direction Y;
    public BitSet Pressed;
    public BitSet Released;

    public readonly bool IsEmpty()
    {
        return X == Direction.Neutral && Y == Direction.Neutral && Pressed.IsEmpty() && Released.IsEmpty();
    }

    public override readonly string ToString()
    {
        string result = "";
        if (X != Direction.Neutral) result += $", X = {X}";
        if (Y != Direction.Neutral) result += $", Y = {Y}";
        if (!Pressed.IsEmpty()) result += $", Pressed = {Pressed}";
        if (!Released.IsEmpty()) result += $", Released = {Released}";
        return result.Length == 0 ? "" : result[2..];
    }
}