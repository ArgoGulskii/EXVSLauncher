using httptest;
using Launcher.Input;
using Launcher.Utils;
using Launcher.Views.Rebind;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;

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
        Save = [];
        Load = [];
        Reset = [];
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
        Save = other.Save;
        Load = other.Load;
        Reset = other.Reset;

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
    public BitSet Save;
    public BitSet Load;
    public BitSet Reset;

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
        Save[button] = false;
        Load[button] = false;
        Reset[button] = false;
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
    public RebindViewModel() : this(0, null!, null!, "", "", null!, null!)
    {
    }

    public RebindViewModel(int id, RebindWindow window, string configPath, string cardPath, string defaultCard, string serverIP, string serverPort)
    {
        id_ = id;
        rebindWindow_ = window;
        configPath_ = configPath;
        cardPath_ = cardPath;
        defaultCard_ = defaultCard;
        cardId_ = defaultCard;
        cardName_ = "-";
        accessCode_ = "1111";   // 1111 is just the default value we set, lol

        CReader.ConnectReader(); // Connect to a cardreader. This will only run once.
        CReader.SetTimeoutWithLock(5000); // Set cardreader timeout to 5 seconds.
        ControllerConfigHttpHelper.SetBaseClient(serverIP, serverPort);

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
        loadCardSelected_ = index.Select(idx => idx == 13).ToProperty(this, x => x.SaveCardSelected);
        saveCardSelected_ = index.Select(idx => idx == 14).ToProperty(this, x => x.LoadCardSelected);
        removeCardSelected_ = index.Select(idx => idx == 15).ToProperty(this, x => x.RemoveCardSelected);

        if (defaultCard_ == "")
        {
            cardName_ = "DISABLED";
        }

        Reset();
    }

    public override string ToString()
    {
        return $"RebindViewModel({id_})";
    }

    public void Reset()
    {
        ModalText = "Do not press any buttons";
        ModalVisible = true;

        SelectedIndex = 0;
        lastState_ = new();

        ChangePresets(RebindBindings.PresetPSStick);
    }

    public void Start()
    {
        Console.WriteLine($"{this}: started");

        cardId_ = defaultCard_;
        cardName_ = "-";
        accessCode_ = "1111";   // 1111 is just the default value we set, lol

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
        Console.WriteLine($"{this}: finished");

        active_ = false;
        rebindWindow_.Hide();
        var config = new ConfigIni()
        {
            ControllerEnabled = true,
            ControllerPath = controllerPath_,
            Bindings = Bindings.ToInputBindings(),
        };
        config.Write(configPath_);

        if (defaultCard_ != "")
        {
            var card = new CardIni()
            {
                CardId = cardId_,
                AccessCode = accessCode_,
            };

            card.Write(cardPath_);
        }
    }

    private static bool HasBinding(int index)
    {
        return index >= 1 && index <= 10;
    }

    private BitSet GetBinding(int index, RebindBindings binds)
    {
        switch (index)
        {
            case 1: return binds.Main;
            case 2: return binds.Melee;
            case 3: return binds.Boost;
            case 4: return binds.Switch;
            case 5: return binds.Sub;
            case 6: return binds.SpecialShooting;
            case 7: return binds.SpecialMelee;
            case 8: return binds.Burst;
            case 9: return binds.Start;
            case 10: return binds.Card;
            default: throw new InvalidOperationException($"GetBinding called on invalid row {index}"); ;
        }
    }

    private void SetBinding(int index, BitSet value, RebindBindings binds)
    {
        switch (index)
        {
            case 1: binds.Main = value; break;
            case 2: binds.Melee = value; break;
            case 3: binds.Boost = value; break;
            case 4: binds.Switch = value; break;
            case 5: binds.Sub = value; break;
            case 6: binds.SpecialShooting = value; break;
            case 7: binds.SpecialMelee = value; break;
            case 8: binds.Burst = value; break;
            case 9: binds.Start = value; break;
            case 10: binds.Card = value; break;
            default: throw new InvalidOperationException($"SetBinding called on invalid row {index}");
        }
    }

    private void ClearBinding(int index)
    {
        if (HasBinding(index)) SetBinding(index, [], Bindings);
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
            11 => this.GetType().GetProperty("Blackhole"),
            12 => this.GetType().GetProperty("Blackhole"),
            13 => this.GetType().GetProperty("CardName"),
            14 => this.GetType().GetProperty("Blackhole"),
            15 => this.GetType().GetProperty("Blackhole"),
            _ => null,
        };
    }

    private void UpdateBindingText()
    {
        for (int i = 1; i < 11; ++i)
        {
            var binding = GetBinding(i, Bindings);
            var property = GetBindingTextProperty(i);
            property!.SetValue(this, string.Join(" ", binding));
        }
    }

    public void HandleUp()
    {
        int i = (SelectedIndex - 1) % 16;
        if (i < 0) i += 16;
        SelectedIndex = i;
    }

    public void HandleDown()
    {
        SelectedIndex = (SelectedIndex + 1) % 16;
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
            var bindings = GetBinding(i, Bindings)!;
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
        ControllerText = $"{device.ProductName} ({device.VendorId:X4}:{device.ProductId:X4})";
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

        // If waiting on card functions, reject all input except to cancel card functions.
        if (waitingCard_ && DateTime.UtcNow - cardTime > TimeSpan.FromMilliseconds(500))
        {
            var startTime = DateTime.UtcNow;
            foreach (int button in diff.Pressed)
            {
                if (Bindings.Main[button])
                {
                    // Set flag to cancel ongoing card operations if possible.
                    CReader.SetCancel(id_);
                }
            }
            return;
        }

        if (diff.X == Direction.Negative) HandleLeft();
        if (diff.X == Direction.Positive) HandleRight();
        if (diff.Y == Direction.Negative) HandleUp();
        if (diff.Y == Direction.Positive) HandleDown();

        if (HasBinding(SelectedIndex))
        {
            var binding = GetBinding(SelectedIndex, Bindings);
            foreach (int button in diff.Pressed)
            {
                Bindings.RemoveFromAll(button);
                binding[button] = true;
            }
            SetBinding(SelectedIndex, binding, Bindings);
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


        foreach (int button in diff.Pressed)
        {
            if (Bindings.Main[button])
            {
                if (SaveSelected) // (SelectedIndex == 12)
                    {
                    Finish();
                    break;
                }

                if (LoadCardSelected && defaultCard_ != "")
                {
                    waitingCard_ = true;
                    cardTime = DateTime.UtcNow;
                    // Start a thread to load a smartcard; we ignore exceptions/returns from this thread.
                    Task.Run(() => loadCard());
                }

                if (SaveCardSelected && defaultCard_ != "" && cardId_ != defaultCard_)
                {
                    waitingCard_ = true;
                    cardTime = DateTime.UtcNow;
                    // Start a thread to save configs to a smartcard; we ignore exceptions/returns from this thread.
                    Task.Run(() => saveCard());
                }

                if (RemoveCardSelected && defaultCard_ != "")
                {
                    cardId_ = defaultCard_;
                    CardName = "-";
                    ChangePresets(RebindBindings.PresetPSStick);
                }
            }
        }
    }

    private RebindBindings ControllerConfigToRebindBindings(ControllerConfig cc)
    {
        RebindBindings rb = new();
        // A Key
        var binding = GetBinding(1, rb);
        foreach (int button in cc.AKey)
        {
            binding[button] = true;
        }
        SetBinding(1, binding, rb);

        // B Key
        binding = GetBinding(2, rb);
        foreach (int button in cc.BKey)
        {
            binding[button] = true;
        }
        SetBinding(2, binding, rb);

        // C Key
        binding = GetBinding(3, rb);
        foreach (int button in cc.CKey)
        {
            binding[button] = true;
        }
        SetBinding(3, binding, rb);

        // D Key
        binding = GetBinding(4, rb);
        foreach (int button in cc.DKey)
        {
            binding[button] = true;
        }
        SetBinding(4, binding, rb);

        // Start Key
        binding = GetBinding(9, rb);
        foreach (int button in cc.StartKey)
        {
            binding[button] = true;
        }
        SetBinding(9, binding, rb);

        // Card Key
        binding = GetBinding(10, rb);
        foreach (int button in cc.CardKey)
        {
            binding[button] = true;
        }
        SetBinding(10, binding, rb);

        return rb;
    }

    private ControllerConfig RebindBindingsToControllerConfig(RebindBindings rb)
    {
        ControllerConfig cc = new();
        // A Key
        var binding = GetBinding(1, rb);
        cc.AKey = binding.ToArray<int>();
        // B Key
        binding = GetBinding(2, rb);
        cc.BKey = binding.ToArray<int>();
        // C Key
        binding = GetBinding(3, rb);
        cc.CKey = binding.ToArray<int>();
        // D Key
        binding = GetBinding(4, rb);
        cc.DKey = binding.ToArray<int>();
        // Start Key
        binding = GetBinding(9, rb);
        cc.StartKey = binding.ToArray<int>();
        // Card Key
        binding = GetBinding(10, rb);
        cc.CardKey = binding.ToArray<int>();

        return cc;
    }

    // All of the card related functions (load/saveCard) need to be run async
    private async Task loadCard()
    {
        string tempName = CardName;
        CardName = "TAP CARD";
        try
        {
            CardReaderResponse response = CReader.GetUUIDWithLockAndID(id_);

            // If the given card isn't registered, restore the previous card text and cancel card operations.
            if (!response.Success)
            {
                CardName = tempName;
                waitingCard_ = false;
                return;
            }

            // If the card is registered, change the card name to the player's name, regardless of whether there's a saved controller config.
            cardId_ = response.Uuid.ToString();
            CardInfo? ci = await ControllerConfigHttpHelper.GetCardInfo(cardId_);
            CardName = cardId_;

            ControllerConfig? cc = await ControllerConfigHttpHelper.GetControllerConfig(cardId_);
            if (cc.HasValue)
            {
                RebindBindings cardBindings = ControllerConfigToRebindBindings(cc.Value);
                cardBindings.Name = ci.Value.Name + " (Custom)";
                ChangePresets(cardBindings);
            }
            else
            {
                PresetText = "NO PROFILE";
                UpdateBindingText();
            }
        }
        catch (Exception ex)
        {
            CardName = tempName;
            Console.WriteLine("Error during card read, likely due to card being removed mid-read. Read cancelled: " + ex.ToString());
        }
        
        waitingCard_ = false;
    }
    private async Task saveCard()
    {
        // Attempt to save the user's current button layout to the card.
        bool success = await ControllerConfigHttpHelper.SendControllerConfig(cardId_, RebindBindingsToControllerConfig(Bindings));

        // If successful, briefly show "SAVED" to indicate a successful save, otherwise show "ERROR".
        if (success)
        {
            Console.WriteLine("Saved controller config to card " + cardId_ + " [" + CardName + "]");
            var tempName = CardName;
            CardName = "SAVED";
            Thread.Sleep(50);
            CardName = tempName;
        }
        else
        {
            Console.WriteLine("WARNING: Failed to save controller config to card " + cardId_ + " [" + CardName + "]");
            var tempName = CardName;
            CardName = "ERROR";
            Thread.Sleep(50);
            CardName = tempName;
        }

        waitingCard_ = false;
    }

    DateTime cardTime = DateTime.UtcNow;
    private bool active_ = false;
    private InputState lastState_ = new();
    private int id_;
    private string cardId_;
    private string accessCode_;

    private bool waitingCard_ = false;

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
    private static readonly CardReader CReader = new();
    private static ControllerConfigHttpHelper ControllerConfigHttpHelper = new();

    private string controllerText_ = "Unknown/Missing Device"; // HORI FIGHTING STICK α (0f0d:011c)
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
    private readonly string cardPath_;
    private string? controllerPath_;
    private string defaultCard_;

    readonly ObservableAsPropertyHelper<bool> loadCardSelected_;
    public bool LoadCardSelected => loadCardSelected_.Value;

    readonly ObservableAsPropertyHelper<bool> saveCardSelected_;
    public bool SaveCardSelected => saveCardSelected_.Value;
    private string cardName_;
    public string CardName
    {
        get => cardName_;
        set => this.RaiseAndSetIfChanged(ref cardName_, value);
    }

    readonly ObservableAsPropertyHelper<bool> removeCardSelected_;
    public bool RemoveCardSelected => removeCardSelected_.Value;

    public Bitmap? HeaderBitmap { get; } = HeaderImage.Get();

    private string blackhole_ = "";
    public string Blackhole
    {
        get => blackhole_;
        set => this.RaiseAndSetIfChanged(ref blackhole_, value);
    }
}