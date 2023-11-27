using Avalonia.Data;
using Avalonia.Media.Imaging;
using Launcher.Input;
using Launcher.Views.Rebind;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace Launcher.ViewModels;

public class RebindBindings
{
    public RebindBindings()
    {
        Name = "";
        Main = [];
        Melee = [];
        Boost = [];
        Switch = [];
        Sub = [];
        SpecialShooting = [];
        SpecialMelee = [];
        Burst = [];
        Start = [];
        Card = [];
        Test = [];
    }


    public void Assign(RebindBindings other)
    {
        Name = other.Name;
        Main = other.Main;
        Melee = other.Melee;
        Boost = other.Boost;
        Switch = other.Switch;
        Sub = other.Sub;
        SpecialShooting = other.SpecialShooting;
        SpecialMelee = other.SpecialMelee;
        Burst = other.Burst;
        Start = other.Start;
        Card = other.Card;
        Test = other.Test;

        Next = other.Next;
        Prev = other.Prev;
    }

    public string Name { get; set; }

    public BitSet Main;
    public BitSet Melee;
    public BitSet Boost;
    public BitSet Switch;
    public BitSet Sub;
    public BitSet SpecialShooting;
    public BitSet SpecialMelee;
    public BitSet Burst;
    public BitSet Start;
    public BitSet Card;
    public BitSet Test;

    public RebindBindings? Base { get; set; }
    public RebindBindings? Next { get; set; }
    public RebindBindings? Prev { get; set; }

    public InputBindings ToInputBindings()
    {
        var result = new InputBindings
        {
            Main = Main | Sub | SpecialShooting | Burst,
            Melee = Melee | Sub | SpecialMelee | Burst,
            Boost = Boost | SpecialShooting | SpecialMelee | Burst,
            Switch = Switch,

            Start = Start,
            Card = Card,
            Test = Test
        };

        return result;
    }

    public void RemoveFromAll(int button)
    {
        Main[button] = false;
        Melee[button] = false;
        Boost[button] = false;
        Switch[button] = false;
        Sub[button] = false;
        SpecialShooting[button] = false;
        SpecialMelee[button] = false;
        Burst[button] = false;
        Start[button] = false;
        Card[button] = false;
        Test[button] = false;
    }

    public readonly static RebindBindings PresetPSStick = new()
    {
        Name = "PS3/PS4 Arcade Stick",
        Main = [1],
        Melee = [4],
        Boost = [6],
        Switch = [2],
        Start = [10],
        Card = [13],
    };

    public readonly static RebindBindings PresetPSController = new()
    {
        Name = "PS3/PS4 Controller",
        Main = [1],
        Melee = [4],
        Boost = [2],
        Switch = [3],
        Sub = [6],
        SpecialShooting = [7],
        SpecialMelee = [8],
        Burst = [12],

        Start = [5, 10],
        Card = [11],
    };

    static RebindBindings()
    {
        PresetPSStick.Next = PresetPSController;
        PresetPSStick.Prev = PresetPSController;

        PresetPSController.Next = PresetPSStick;
        PresetPSController.Prev = PresetPSStick;
    }
}

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public partial class RebindViewModel : ViewModelBase
{
    public RebindViewModel() : this(null!, null!)
    {
    }

    public RebindViewModel(RebindWindow window, string configPath)
    {
        rebindWindow_ = window;
        configPath_ = configPath;

        var index = this.WhenAnyValue(x => x.SelectedIndex);
        presetSelected_ = index.Select(idx => idx == 0).ToProperty(this, x => x.PresetSelected);
        mainSelected_ = index.Select(idx => idx == 1).ToProperty(this, x => x.MainSelected);
        meleeSelected_ = index.Select(idx => idx == 2).ToProperty(this, x => x.MeleeSelected);
        boostSelected_ = index.Select(idx => idx == 3).ToProperty(this, x => x.BoostSelected);
        switchSelected_ = index.Select(idx => idx == 4).ToProperty(this, x => x.SwitchSelected);
        subSelected_ = index.Select(idx => idx == 5).ToProperty(this, x => x.SubSelected);
        specialShootingSelected_ = index.Select(idx => idx == 6).ToProperty(this, x => x.SpecialShootingSelected);
        specialMeleeSelected_ = index.Select(idx => idx == 7).ToProperty(this, x => x.SpecialMeleeSelected);
        burstSelected_ = index.Select(idx => idx == 8).ToProperty(this, x => x.BurstSelected);
        startSelected_ = index.Select(idx => idx == 9).ToProperty(this, x => x.StartSelected);
        cardSelected_ = index.Select(idx => idx == 10).ToProperty(this, x => x.CardSelected);
        testSelected_ = index.Select(idx => idx == 11).ToProperty(this, x => x.TestSelected);
        saveSelected_ = index.Select(idx => idx == 12).ToProperty(this, x => x.SaveSelected);

        Reset();
    }

    public void Reset()
    {
        ModalText = "Do not press any buttons";
        ModalVisible = true;

        SelectedIndex = 0;
        lastState_ = new();

        ChangePresets(RebindBindings.PresetPSStick);
        UpdateBindingText();
    }

    public void Start()
    {
        rebindWindow_.Show();
        var config = new ConfigIni()
        {
            ControllerEnabled = false,
        };
        config.Write(configPath_);
        active_ = true;
    }

    public void Finish()
    {
        active_ = false;
        rebindWindow_.Hide();
        var config = new ConfigIni()
        {
            ControllerEnabled = true,
            ControllerPath = controllerPath_,
            Bindings = Bindings.ToInputBindings(),
        };
        config.Write(configPath_);
    }

    private static bool HasBinding(int index)
    {
        return index >= 1 && index <= 10;
    }

    private BitSet GetBinding(int index)
    {
        switch (index)
        {
            case 1: return Bindings.Main;
            case 2: return Bindings.Melee;
            case 3: return Bindings.Boost;
            case 4: return Bindings.Switch;
            case 5: return Bindings.Sub;
            case 6: return Bindings.SpecialShooting;
            case 7: return Bindings.SpecialMelee;
            case 8: return Bindings.Burst;
            case 9: return Bindings.Start;
            case 10: return Bindings.Card;
            default: throw new InvalidOperationException($"GetBinding called on invalid row {index}"); ;
        }
    }

    private void SetBinding(int index, BitSet value)
    {
        switch (index)
        {
            case 1: Bindings.Main = value; break;
            case 2: Bindings.Melee = value; break;
            case 3: Bindings.Boost = value; break;
            case 4: Bindings.Switch = value; break;
            case 5: Bindings.Sub = value; break;
            case 6: Bindings.SpecialShooting = value; break;
            case 7: Bindings.SpecialMelee = value; break;
            case 8: Bindings.Burst = value; break;
            case 9: Bindings.Start = value; break;
            case 10: Bindings.Card = value; break;
            default: throw new InvalidOperationException($"SetBinding called on invalid row {index}");
        }
    }

    private void ClearBinding(int index)
    {
        if (HasBinding(index)) SetBinding(index, []);
    }

    private PropertyInfo? GetBindingTextProperty(int index)
    {
        return index switch
        {
            1 => this.GetType().GetProperty("MainText"),
            2 => this.GetType().GetProperty("MeleeText"),
            3 => this.GetType().GetProperty("BoostText"),
            4 => this.GetType().GetProperty("SwitchText"),
            5 => this.GetType().GetProperty("SubText"),
            6 => this.GetType().GetProperty("SpecialShootingText"),
            7 => this.GetType().GetProperty("SpecialMeleeText"),
            8 => this.GetType().GetProperty("BurstText"),
            9 => this.GetType().GetProperty("StartText"),
            10 => this.GetType().GetProperty("CardText"),
            _ => null,
        };
    }

    private void UpdateBindingText()
    {
        for (int i = 1; i < 11; ++i)
        {
            var binding = GetBinding(i);
            var property = GetBindingTextProperty(i);
            property!.SetValue(this, string.Join(" ", binding));
        }
    }

    public void HandleUp()
    {
        int i = (SelectedIndex - 1) % 13;
        if (i < 0) i += 13;
        SelectedIndex = i;
    }

    public void HandleDown()
    {
        SelectedIndex = (SelectedIndex + 1) % 13;
    }

    private void ChangePresets(RebindBindings selected)
    {
        Bindings.Assign(selected);
        PresetText = Bindings.Name;
        UpdateBindingText();
    }

    public void HandleLeft()
    {
        if (SelectedIndex == 0)
        {
            ChangePresets(Bindings.Prev!);
        }

        ClearBinding(SelectedIndex);
        UpdateBindingText();
    }

    public void HandleRight()
    {
        if (SelectedIndex == 0)
        {
            ChangePresets(Bindings.Next!);
            return;
        }
        else if (SelectedIndex == 12)
        {
            Finish();
            return;
        }

        ClearBinding(SelectedIndex);
        UpdateBindingText();
    }

    bool UpdateTest(BitSet buttons, List<int> mappingIndices)
    {
        foreach (int i in mappingIndices)
        {
            var bindings = GetBinding(i)!;
            foreach (int button in buttons)
            {
                if (bindings.Contains(button)) return true;
            }
        }
        return false;
    }

    public void DeviceSelected(InputDevice device)
    {
        active_ = true;
        ModalVisible = false;
        ControllerText = $"{device.ManufacturerName} {device.ProductName} ({device.VendorId:X4}:{device.ProductId:X4})";
        controllerPath_ = device.DevicePath;
    }

    public void DeviceAvailable()
    {
        ModalText = "Press any button";
    }

    public void DeviceLeft()
    {
        Reset();
        Start();
    }

    public void HandleInput(InputState input)
    {
        if (!active_) return;

        var diff = lastState_.Diff(input);
        lastState_ = input;

        if (diff.X == Direction.Negative) HandleLeft();
        if (diff.X == Direction.Positive) HandleRight();
        if (diff.Y == Direction.Negative) HandleUp();
        if (diff.Y == Direction.Positive) HandleDown();

        if (HasBinding(SelectedIndex))
        {
            var binding = GetBinding(SelectedIndex);
            foreach (int button in diff.Pressed)
            {
                Bindings.RemoveFromAll(button);
                binding[button] = true;
            }
            SetBinding(SelectedIndex, binding);
            UpdateBindingText();
        }

        if (TestSelected)
        {
            var bindings = Bindings.ToInputBindings();
            TestA = !(input.Buttons & bindings.Main).IsEmpty();
            TestB = !(input.Buttons & bindings.Melee).IsEmpty();
            TestC = !(input.Buttons & bindings.Boost).IsEmpty();
            TestD = !(input.Buttons & bindings.Switch).IsEmpty();
            TestStart = !(input.Buttons & bindings.Start).IsEmpty();
            TestCard = !(input.Buttons & bindings.Card).IsEmpty();
        }

        if (SelectedIndex == 12)
        {
            foreach (int button in diff.Pressed)
            {
                if (Bindings.Main[button])
                {
                    Finish();
                    break;
                }
            }
        }
    }

    private bool active_ = false;
    private InputState lastState_ = new();

    private bool modalVisible_ = true;
    public bool ModalVisible
    {
        get => modalVisible_;
        set => this.RaiseAndSetIfChanged(ref modalVisible_, value);
    }

    private string modalText_ = "";
    public string ModalText
    {
        get => modalText_;
        set => this.RaiseAndSetIfChanged(ref modalText_, value);
    }

    public readonly RebindBindings Bindings = new();

    private string controllerText_ = "HORI FIGHTING STICK α (0f0d:011c)";
    public string ControllerText
    {
        get => controllerText_;
        set => this.RaiseAndSetIfChanged(ref controllerText_, value);
    }

    public int selectedIndex_ = 0;
    public int SelectedIndex
    {
        get => selectedIndex_;
        set => this.RaiseAndSetIfChanged(ref selectedIndex_, value);
    }

    readonly ObservableAsPropertyHelper<bool> presetSelected_;
    public bool PresetSelected => presetSelected_.Value;
    private string presetText_ = "";
    public string PresetText
    {
        get => presetText_;
        set => this.RaiseAndSetIfChanged(ref presetText_, value);
    }

    readonly ObservableAsPropertyHelper<bool> mainSelected_;
    public bool MainSelected => mainSelected_.Value;
    private string mainText_ = "";
    public string MainText
    {
        get => mainText_;
        set => this.RaiseAndSetIfChanged(ref mainText_, value);
    }

    readonly ObservableAsPropertyHelper<bool> meleeSelected_;
    public bool MeleeSelected => meleeSelected_.Value;
    private string meleeText_ = "";
    public string MeleeText
    {
        get => meleeText_;
        set => this.RaiseAndSetIfChanged(ref meleeText_, value);
    }

    readonly ObservableAsPropertyHelper<bool> boostSelected_;
    public bool BoostSelected => boostSelected_.Value;
    private string boostText_ = "";
    public string BoostText
    {
        get => boostText_;
        set => this.RaiseAndSetIfChanged(ref boostText_, value);
    }

    readonly ObservableAsPropertyHelper<bool> switchSelected_;
    public bool SwitchSelected => switchSelected_.Value;
    private string switchText_ = "";
    public string SwitchText
    {
        get => switchText_;
        set => this.RaiseAndSetIfChanged(ref switchText_, value);
    }

    readonly ObservableAsPropertyHelper<bool> subSelected_;
    public bool SubSelected => subSelected_.Value;
    private string subText_ = "";
    public string SubText
    {
        get => subText_;
        set => this.RaiseAndSetIfChanged(ref subText_, value);
    }

    readonly ObservableAsPropertyHelper<bool> specialShootingSelected_;
    public bool SpecialShootingSelected => specialShootingSelected_.Value;
    private string specialShootingText_ = "";
    public string SpecialShootingText
    {
        get => specialShootingText_;
        set => this.RaiseAndSetIfChanged(ref specialShootingText_, value);
    }

    readonly ObservableAsPropertyHelper<bool> specialMeleeSelected_;
    public bool SpecialMeleeSelected => specialMeleeSelected_.Value;
    private string specialMeleeText_ = "";
    public string SpecialMeleeText
    {
        get => specialMeleeText_;
        set => this.RaiseAndSetIfChanged(ref specialMeleeText_, value);
    }

    readonly ObservableAsPropertyHelper<bool> burstSelected_;
    public bool BurstSelected => burstSelected_.Value;
    private string burstText_ = "";
    public string BurstText
    {
        get => burstText_;
        set => this.RaiseAndSetIfChanged(ref burstText_, value);
    }

    readonly ObservableAsPropertyHelper<bool> startSelected_;
    public bool StartSelected => startSelected_.Value;
    private string startText_ = "";
    public string StartText
    {
        get => startText_;
        set => this.RaiseAndSetIfChanged(ref startText_, value);
    }

    readonly ObservableAsPropertyHelper<bool> cardSelected_;
    public bool CardSelected => cardSelected_.Value;
    private string cardText_ = "";
    public string CardText
    {
        get => cardText_;
        set => this.RaiseAndSetIfChanged(ref cardText_, value);
    }

    readonly ObservableAsPropertyHelper<bool> testSelected_;
    public bool TestSelected => testSelected_.Value;

    private bool testA_;
    public bool TestA
    {
        get => testA_;
        set => this.RaiseAndSetIfChanged(ref testA_, value);
    }

    private bool testB_;
    public bool TestB
    {
        get => testB_;
        set => this.RaiseAndSetIfChanged(ref testB_, value);
    }

    private bool testC_;
    public bool TestC
    {
        get => testC_;
        set => this.RaiseAndSetIfChanged(ref testC_, value);
    }

    private bool testD_;
    public bool TestD
    {
        get => testD_;
        set => this.RaiseAndSetIfChanged(ref testD_, value);
    }

    private bool testStart_;
    public bool TestStart
    {
        get => testStart_;
        set => this.RaiseAndSetIfChanged(ref testStart_, value);
    }

    private bool testCard_;
    public bool TestCard
    {
        get => testCard_;
        set => this.RaiseAndSetIfChanged(ref testCard_, value);
    }

    readonly ObservableAsPropertyHelper<bool> saveSelected_;
    public bool SaveSelected => saveSelected_.Value;

    private readonly RebindWindow rebindWindow_;
    private readonly string configPath_;
    private string? controllerPath_;

    private static Bitmap? GetHeaderImage()
    {
        var imagePath = Path.Join(System.AppContext.BaseDirectory, "header.png");
        if (Path.Exists(imagePath))
        {
            return new Bitmap(imagePath);
        }
        return null;
    }

    public Bitmap? HeaderBitmap { get; } = GetHeaderImage();
}