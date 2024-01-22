using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicData.Kernel;
using Launcher.Input;
using PeanutButter.INI;

namespace Launcher;

internal class ConfigIni
{
    // TODO: Fill in the rest of the configuration items.
    public bool ControllerEnabled;
    public string? ControllerPath;
    public InputBindings Bindings;

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
            audio["Id"] = AudioId;
        }
        else
        {
            audio.Remove("Id");
        }

        ini.Persist();
        Console.WriteLine($"Wrote config to {path}");
    }
}
