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


    private (string User, string Message) CleanMessage(string rawMessage)
    {

      return (
        reUserName.Match(rawMessage).Groups[1].Value,
        reChatMessage.Match(rawMessage).Groups[1].Value
        );

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
