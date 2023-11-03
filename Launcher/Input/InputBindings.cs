using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher.Input;

public struct InputBindings
{
    public InputBindings() { }

    public BitSet Main = [];
    public BitSet Melee = [];
    public BitSet Boost = [];
    public BitSet Switch = [];
    public BitSet Start = [];
    public BitSet Card = [];
    public BitSet Test = [];
}
