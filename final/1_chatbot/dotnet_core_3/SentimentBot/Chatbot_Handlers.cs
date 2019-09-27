using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Components;
using System.Text.Json;

namespace SentimentBot
{
  public partial class ChatBot
  {

    private static HttpClient _SentimentClient;

    static ChatBot()
    {
      CreateNewClient();
    }

    public static bool HandlePong(string message, ILogger logger, Action<string> send)
    {

      if (!message.StartsWith("PING")) return false;

      logger.LogWarning("Received PING from Twitch... sending PONG");
      send($"PONG :{message.Split(':')[1]}");
      return true;

    }

    public void AnalyzeSentiment(string rawMessage)
    {

      var message = CleanMessage(rawMessage);
      if (message.User == BotName) return;

      if (_SentimentClient == null) CreateNewClient();

      var payload = JsonSerializer.Serialize(new ChatPayload{ SentimentText= message.Message, UserName=message.User, Channel=ChannelName });
      var httpContent = new StringContent(payload, Encoding.UTF8, @"application/json");
      var msg = _SentimentClient.PostAsync("", httpContent).GetAwaiter().GetResult();
        
      var result = msg.Content.ReadAsStringAsync().GetAwaiter().GetResult();
      _Logger.LogInformation($"Sentiment of {result} from {message.User}");

    }

    private static void CreateNewClient()
    {

      _SentimentClient = new HttpClient
      {
        BaseAddress = new Uri("https://twitchcon.csharpfritz.com/sentiment")
      };


    }
  }

}
