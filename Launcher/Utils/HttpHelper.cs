using Launcher.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Launcher.Utils;

public struct CardInfo
{
    public string Name;
    public string AccessCode;
}
public class HttpHelper
{
    public static CardInfo GetCardInfo(string cardId, string serverIp, string serverPort)
    {
        return new CardInfo() {
            Name = "Not implemented",
            AccessCode = "0000",
        };
    }

    public static bool SendControllerConfig(string cardId, InputBindings bindings, string serverIp, string serverPort)
    {
        return false;
    }
}
