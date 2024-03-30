using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace httptest;

public struct CardInfo
{
    public string Name;
    public string CardId;
}

public struct ControllerConfig
{
    public int[] AKey;
    public int[] BKey;
    public int[] CKey;
    public int[] DKey;
    public int[] StartKey;
    public int[] CardKey;
}

public class ControllerConfigHttpHelper
{
    private static HttpClient? sharedClient;
    private Mutex mu = new Mutex();

    public void SetBaseClient(string serverIp, string serverPort)
    {
        if (mu.WaitOne(0))
        {
            if (serverPort != "")
            {
                sharedClient = new()
                {
                    BaseAddress = new Uri("http://" + serverIp + ":" + serverPort),
                };
            }
            else
            {
                sharedClient = new()
                {
                    BaseAddress = new Uri("http://" + serverIp),
                };
            }
            sharedClient.Timeout = TimeSpan.FromSeconds(3);
            mu.ReleaseMutex();
        }
    }

    public async Task<CardInfo?> GetCardInfo(string cardId)
    {
        if (mu.WaitOne(0))
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
            };

            using HttpResponseMessage response = await sharedClient.GetAsync("card/getBasicDisplayProfileById/" + cardId);

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
                    foreach (KeyValuePair<string, object> kvp in jsondict)
                    {
                        if (kvp.Key == "userName") ci.Name = kvp.Value.ToString();
                    }
                }
            }
            else
            {
                Console.WriteLine("no response display profile");
            }
            mu.ReleaseMutex();
            return ci;
        }
        return null;
    }

    public async Task<ControllerConfig?> GetControllerConfig(string cardId)
    {
        if (mu.WaitOne(0))
        {
            if (sharedClient == null)
            {
                Console.WriteLine("No client\n");
                return null;
            }

            using HttpResponseMessage response = await sharedClient.GetAsync("card/getControllerConfigById/" + cardId);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("failed to get controller config");
                return null;
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            if (jsonResponse != "")
            {
                Console.WriteLine($"config response: '{jsonResponse}'\n");
                var jsondict = JsonSerializer.Deserialize<Dictionary<string, int[]>>(jsonResponse);

                ControllerConfig cc = new();

                if (jsondict != null)
                {
                    jsondict.TryGetValue("aKey", out cc.AKey);
                    jsondict.TryGetValue("bKey", out cc.BKey);
                    jsondict.TryGetValue("cKey", out cc.CKey);
                    jsondict.TryGetValue("dKey", out cc.DKey);
                    jsondict.TryGetValue("startKey", out cc.StartKey);
                    jsondict.TryGetValue("cardKey", out cc.CardKey);

                    return cc;
                }
            }
            Console.WriteLine("no response get controller");
            mu.ReleaseMutex();
            return null;
        }
        return null;
    }

    public async Task<bool> SendControllerConfig(string cardId, ControllerConfig bindings)
    {
        if (mu.WaitOne(0))
        {
            if (sharedClient == null)
            {
                Console.WriteLine("No client\n");
                return false;
            }

            var data = new
            {
                ChipId = cardId,
                ControllerConfig = new
                {
                    AKey = bindings.AKey,
                    BKey = bindings.BKey,
                    CKey = bindings.CKey,
                    DKey = bindings.DKey,
                    StartKey = bindings.StartKey,
                    CardKey = bindings.CardKey
                }
            };

            string jsonData = JsonSerializer.Serialize(data);

            var jsonString = new StringContent(jsonData, Encoding.UTF8, "application/json");

            Console.WriteLine("\n\njson string: " + jsonString.ToString());

            using HttpResponseMessage response = await sharedClient.PostAsync("card/updateControllerConfig/", jsonString);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("failed to update controller config");
                return false;
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            if (jsonResponse != "")
            {
                Console.WriteLine($"update response: '{jsonResponse}'\n");
            }
            else
            {
                Console.WriteLine("no response set controller");
            }
            mu.ReleaseMutex();
            return false;
        }
        return false;
    }
}
