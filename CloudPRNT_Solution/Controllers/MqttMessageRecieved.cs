using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CloudPRNT_Solution.Controllers;

    public class MqttMessageReciebed : Controller
    {
        
        // public async void HandleReceivedMqttMessage(IDbConnection db, string mqttTopic, string mqttPayload)
        // {
        //     if (string.IsNullOrEmpty(mqttTopic) || string.IsNullOrEmpty(mqttPayload))
        //     {
        //         // Could not parse MQTT message properly.
        //         return;
        //     }

        //     var parsed = JsonConvert.DeserializeObject<Dictionary<string, object>>(mqttPayload);

        //     if (parsed.ContainsKey("title"))
        //     {
        //         var title = parsed["title"].ToString();

        //         if (title == "client-status")
        //         {
        //             HandleClientStatus(db, parsed);
        //         }
        //         else if (title == "print-result")
        //         {
        //             HandlePrintResult(db, parsed);
        //         }
        //         else
        //         {
        //             // Ignore
        //         }
        //     }
        // }

        
    }
