using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Launcher.Utils;

public struct CardInfo
{
    public string Name;
    public string CardId;
    public bool Exists;
}

public struct ControllerConfig
{
    public int[] AKey;
    public int[] BKey;
    public int[] CKey;
    public int[] DKey;
    public int[] SubKey;
    public int[] SpecialShootingKey;
    public int[] SpecialMeleeKey;
    public int[] BurstKey;
    public int[] StartKey;
    public int[] CardKey;
}

public class ControllerConfigHttpHelper
{
    private static HttpClient? sharedClient;

    public void SetBaseClient(string serverHost)
    {
        sharedClient = new()
        {
            BaseAddress = new Uri("http://" + serverHost),
            Timeout = TimeSpan.FromSeconds(5),
        };
    }

    public async Task<CardInfo?> GetCardInfo(string cardId)
    {
        if (sharedClient == null)
        {
            Console.WriteLine("No client\n");
            return null;
        }

        var ci = new CardInfo()
        {
            Name = "UNKNOWN",
            CardId = cardId,
            Exists = false,
        };

        using HttpResponseMessage response = await sharedClient.GetAsync("ui/card/getBasicDisplayProfileById/" + cardId);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine("failed to get display profile");
            return ci;
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        if (jsonResponse != "")
        {
            Console.WriteLine($"profile response: '{jsonResponse}'\n");
            var jsondict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse);
            if (jsondict != null)
            {
                var name = jsondict["userName"];
                if (name != null)
                {
                    ci.Name = name.ToString();
                    ci.Exists = true;
                }
            }
        }
        else
        {
            Console.WriteLine("no response display profile");
        }
        return ci;
    }

    public async Task<ControllerConfig?> GetControllerConfig(string cardId)
    {
        if (sharedClient == null)
        {
            Console.WriteLine("No client\n");
            return null;
        }

        using HttpResponseMessage response = await sharedClient.GetAsync("ui/controller/get/" + cardId);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine("failed to get controller config");
            return null;
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        if (jsonResponse == "")
        {
            Console.WriteLine("GetControllerConfig: empty json response");
            return null;
        }

        Console.WriteLine($"config response: '{jsonResponse}'\n");
        var jsonstring = JsonSerializer.Deserialize<string>(jsonResponse);
        if (jsonstring == "")
        {
            Console.WriteLine("GetControllerConfig: empty json string");
            return null;
        }

        var jsondict = JsonSerializer.Deserialize<Dictionary<string, int[]>>(jsonstring);

        ControllerConfig cc = new();

        if (jsondict == null)
        {
            Console.WriteLine("GetControllerConfig: no json dict");
            return null;
        }

        jsondict.TryGetValue("AKey", out cc.AKey);
        jsondict.TryGetValue("BKey", out cc.BKey);
        jsondict.TryGetValue("CKey", out cc.CKey);
        jsondict.TryGetValue("DKey", out cc.DKey);
        jsondict.TryGetValue("SubKey", out cc.SubKey);
        jsondict.TryGetValue("SpecialShootingKey", out cc.SpecialShootingKey);
        jsondict.TryGetValue("SpecialMeleeKey", out cc.SpecialMeleeKey);
        jsondict.TryGetValue("BurstKey", out cc.BurstKey);
        jsondict.TryGetValue("StartKey", out cc.StartKey);
        jsondict.TryGetValue("CardKey", out cc.CardKey);

        Console.WriteLine("controller config: " + cc);

        return cc;
    }

    public async Task<bool> SendControllerConfig(string cardId, ControllerConfig bindings)
    {
        if (sharedClient == null)
        {
            Console.WriteLine("No client\n");
            return false;
        }

        var config = new
        {
            AKey = bindings.AKey,
            BKey = bindings.BKey,
            CKey = bindings.CKey,
            DKey = bindings.DKey,
            SubKey = bindings.SubKey,
            SpecialShootingKey = bindings.SpecialShootingKey,
            SpecialMeleeKey = bindings.SpecialMeleeKey,
            BurstKey = bindings.BurstKey,
            StartKey = bindings.StartKey,
            CardKey = bindings.CardKey
        };

        var data = new
        {
            ChipId = cardId,
            ControllerConfig = JsonSerializer.Serialize(config),
        };

        string jsonData = JsonSerializer.Serialize(data);

        var jsonString = new StringContent(jsonData, Encoding.UTF8, "application/json");

        Console.WriteLine("\n\njson string: " + jsonString.ToString());

        using HttpResponseMessage response = await sharedClient.PostAsync("ui/controller/set", jsonString);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine("failed to update controller config");
            return false;
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        if (jsonResponse != "")
        {
            Console.WriteLine($"update response: '{jsonResponse}'\n");
            return true;
        }
        else
        {
            Console.WriteLine("no response when saving controller");
        }
        return false;
    }

    public async Task<String> PreLoadCard(String id, String access)
    {
        if (sharedClient == null)
        {
            Console.WriteLine("No client\n");
            return "";
        }


        var data = new
        {
            Type = 100,
            RequestId = "MTHD_PRE_LOAD_CARD-8",
            pre_load_card = new
            {
                AccessCode = access,
                ChipId = id,
                IsCard = true,
                MuchaCountryCode = "JPN",
            },
        };

        string jsonData = JsonSerializer.Serialize(data);

        var jsonString = new StringContent(jsonData, Encoding.UTF8, "application/json");

        var jsonStringOutput = await jsonString.ReadAsStringAsync();

        Console.WriteLine("\n\nsending json string: " + jsonStringOutput);

        using HttpResponseMessage response = await sharedClient.PostAsync("", jsonString);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine("failed to preload card");
            return "";
        }

        var content = response.Content;
        var byteArray = await content.ReadAsByteArrayAsync();
        var responseString = Encoding.ASCII.GetString(byteArray, 0, byteArray.Length);
        var sessionId = _parseSessionId(byteArray);
        Console.WriteLine("preload session id: " + sessionId);



        if (sessionId != "")
        {
            Console.WriteLine($"preload card response: '{sessionId}'\n");
            return sessionId;
        }
        else
        {
            Console.WriteLine("no response preload card");
        }
        return "";
    }

    private static String _parseSessionId(byte[] byteArray)
    {
        var byteString = BitConverter.ToString(byteArray).Replace("-", "");


        // Cursed stuff because not supporting protobufs.
        var start = "A00100E244140A0E";
        var end = "30014000";

        var startIdx = byteString.LastIndexOf(start);
        var endIdx = byteString.LastIndexOf(end);

        if (startIdx == -1 || endIdx == -1)
        {
            return "";
        }

        var startLastByteIdx = (startIdx + start.Length) / 2;
        var endByteCount = (byteString.Length - endIdx) / 2;


        var response = Encoding.ASCII.GetString(byteArray, startLastByteIdx, byteArray.Length - endByteCount - startLastByteIdx);
        return response;
    }

    public async Task<bool> RegisterCard(String id, String access, String session)
    {
        if (sharedClient == null)
        {
            Console.WriteLine("No client\n");
            return false;
        }

        var data = new
        {
            Type = 102,
            RequestId = "MTHD_REGISTER_CARD-9",
            PcbSerial = "1",
            LocId = "JPN0JPN1",
            AmId = "0",
            register_card = new
            {
                SessionId = session,
                AccessCode = access,
                ChipId = id,
                IsCard = true,
                MuchaCountryCode = "JPN",
            },
        };

        string jsonData = JsonSerializer.Serialize(data);

        var jsonString = new StringContent(jsonData, Encoding.UTF8, "application/json");

        var jsonStringOutput = await jsonString.ReadAsStringAsync();

        Console.WriteLine("\n\nsending json string: " + jsonStringOutput);

        using HttpResponseMessage response = await sharedClient.PostAsync("", jsonString);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine("failed to register card");
            return false;
        }

        var content = response.Content;
        var byteArray = await content.ReadAsByteArrayAsync();
        var responseString = Encoding.ASCII.GetString(byteArray, 0, byteArray.Length);

        if (responseString.IndexOf(session) >= 0)
        {
            Console.WriteLine("Register success, session id matched");
            return true;
        }

        Console.WriteLine("Register failed, no match for session id");
        return false;
    }
}
