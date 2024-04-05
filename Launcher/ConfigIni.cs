using System;
using Launcher.Input;
using PeanutButter.INI;

namespace Launcher;

internal class ConfigIni
{
    // TODO: Fill in the rest of the configuration items.
    public bool ControllerEnabled;
    public string? ControllerPath;
    public InputBindings Bindings;

    public string? IpOrInterface;
    public bool UseInterface;
    public string? Server;

    public string? AudioId;

    public void Write(string path)
    {
        var ini = new INIFile(path);
        ini.WrapValueInQuotes = false;
        if (!ini.HasSection("controller"))
            ini.AddSection("controller");

        var section = ini.GetSection("controller");
        section["Enabled"] = ControllerEnabled ? "true" : "false";
        if (ControllerPath != null)
        {
            section["Path"] = ControllerPath;
        }
        else
        {
            section.Remove("Path");
        }

        section["A"] = string.Join(",", Bindings.Main);
        section["B"] = string.Join(",", Bindings.Melee);
        section["C"] = string.Join(",", Bindings.Boost);
        section["D"] = string.Join(",", Bindings.Switch);
        section["Start"] = string.Join(",", Bindings.Start);
        section["Card"] = string.Join(",", Bindings.Card);
        section["Test"] = string.Join(",", Bindings.Test);


        if (!ini.HasSection("audio"))
            ini.AddSection("audio");

        var audio = ini.GetSection("audio");
        if (AudioId != null)
        {
            audio["Device"] = AudioId;
        }
        else
        {
            audio.Remove("Device");
        }

        if (audio.ContainsKey("Id")) audio.Remove("Id");

        ini.Persist();
        Console.WriteLine($"Wrote config to {path}");
    }

    public void WriteNetwork(string path)
    {
        var ini = new INIFile(path);
        ini.WrapValueInQuotes = false;
        if (!ini.HasSection("config"))
            ini.AddSection("config");

        var section = ini.GetSection("config");
        if (IpOrInterface != null && IpOrInterface != "")
        {
            if (UseInterface)
            {
                section["InterfaceName"] = IpOrInterface;
                section.Remove("IpAddress");
            }
            else
            {
                section["IpAddress"] = IpOrInterface;
                section.Remove("InterfaceName");
            }
        }

        if (Server != null && Server != "")
        {
            section["Server"] = Server;
        }

        ini.Persist();
        Console.WriteLine($"Wrote network config to {path}");
    }
}

internal class CardIni
{
    public string CardId = "";
    public string AccessCode = "";

    public void Write(string path)
    {
        if (CardId == "" || AccessCode == "")
        {
            Console.WriteLine($"Missing card information, skipping write");
            return;
        }
        var ini = new INIFile(path);
        ini.WrapValueInQuotes = false;
        if (!ini.HasSection("card"))
            ini.AddSection("card");

        var section = ini.GetSection("card");
        section["accessCode"] = AccessCode;
        section["chipId"] = CardId;

        ini.Persist();
        Console.WriteLine($"Wrote card to {path}");
    }
}
