using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SentimentBot
{
  public partial class ChatBot
  {
    private CancellationToken _Token;


    internal static readonly Regex reUserName = new Regex(@"!([^@]+)@");
    internal static readonly Regex reBadges = new Regex(@"@badges=([^;]*)");
    internal static Regex reChatMessage;
    internal static Regex reWhisperMessage;

    private static TcpClient _Client;
    private StreamWriter _OutputStream;
    private readonly IConfiguration _Configuration;
    private readonly ILogger<Worker> _Logger;

    private DateTime _NextReset = DateTime.MinValue;
    const int MAXIMUMCOMMANDS = 100;
    private int _CommandCount = 0;
    private Queue<string> _CommandQueue = new Queue<string>();
    private Task _RetryTask;

    public ChatBot(IConfiguration configuration, ILogger<Worker> logger)
    {

      _Configuration = configuration;
      _Logger = logger;
      reChatMessage = new Regex($@"PRIVMSG #{ChannelName} :(.*)$");
      reWhisperMessage = new Regex($@"WHISPER {BotName} :(.*)$");

    }

    internal string AccessToken { get { return _Configuration["Bot:AccessToken"]; } }

    internal string BotName { get { return _Configuration["Bot:Name"]; } }

    internal string ChannelName { get { return _Configuration["Bot:Channel"]; } }

    internal string ChannelId { get { return _Configuration["Bot:ChannelId"]; } }

    internal string ChatroomId { get { return _Configuration["Bot:ChatroomId"]; } }

    public async Task Start(CancellationToken cancellationToken)
    {

      _Client = new TcpClient("irc.chat.twitch.tv", 80);
      var inputStream = new StreamReader(_Client.GetStream());
      _OutputStream = new StreamWriter(_Client.GetStream());

      Login();

      _RetryTask = Task.Run(() => StartRetryQueue(cancellationToken));

      while (!cancellationToken.IsCancellationRequested)
      {

        await Task.Delay(50);
        string message = string.Empty;

        if (_Client.Connected && _Client.Available > 0)
        {
          try
          {
            message = inputStream.ReadLine();
            message = message ?? "";
            _Logger.LogInformation(message);

            await ProcessMessage(message);

          }
          catch (Exception ex)
          {
            _Logger.LogError(ex, "Error reading messages");
          }
        }
        else if (!_Client.Connected)
        {
          // Reconnect
          _Logger.LogWarning("Disconnected from Twitch... attempting to reconnect");
          await Task.Delay(2000);
          Login();
        }


      }

      _Client.Close();

    }

    private async Task StartRetryQueue(CancellationToken cancellationToken)
    {

      while (!cancellationToken.IsCancellationRequested)
      {

        await Task.Delay(100);
        string message = string.Empty;

        if (_CommandQueue.Count > 0 && _NextReset < DateTime.UtcNow)
        {
          message = _CommandQueue.Dequeue();
          PostMessage(message);
        }

      }


    }

    private async Task ProcessMessage(string message)
    {

      if (HandlePong(message, _Logger, (s => SendRawIrcMessage(s)))) return;

      if (reChatMessage.IsMatch(message)) AnalyzeSentiment(message);

      await Task.CompletedTask;

    }


    private void Login()
    {

      //_Client.Connect("irc.chat.twitch.tv", 6697);

      _OutputStream.WriteLine("CAP REQ :twitch.tv/tags twitch.tv/commands twitch.tv/membership");
      _OutputStream.WriteLine($"PASS oauth:{AccessToken}");
      _OutputStream.WriteLine($"NICK {BotName}");
      _OutputStream.WriteLine($"USER {BotName} 8 * :{BotName}");
      _OutputStream.Flush();

      if (string.IsNullOrEmpty(ChatroomId))
      {
        _OutputStream.WriteLine($"JOIN #{ChannelName}");
      } else
      {
        _OutputStream.WriteLine($"JOIN #chatrooms:{ChannelId}:{ChatroomId}");
      }
      _OutputStream.Flush();

    }

    public void PostMessage(string message)
    {

      var fullMessage = $":{BotName}!{BotName}@{BotName}.tmi.twitch.tv PRIVMSG #chatrooms:{ChannelId}:{ChatroomId} :{message}";

      SendRawIrcMessage(fullMessage);

    }

    public void WhisperMessage(string message, string userName)
    {

      var fullMessage = $":{BotName}!{BotName}@{BotName}.tmi.twitch.tv PRIVMSG #jtv :/w {userName} {message}";
      SendRawIrcMessage(fullMessage);

    }

    private void SendRawIrcMessage(string message, bool flush = true)
    {

      var throttled = CheckThrottleStatus();

      if (throttled.HasValue && throttled.Value > TimeSpan.Zero)
      {
        _Logger.LogError($"Bot throttled - waiting {throttled.Value}");
        _CommandQueue.Enqueue(message);
        return;
      }

      try
      {
        _OutputStream.WriteLine(message);
      }
      catch (Exception ex)
      {
        _Logger.LogError(ex, $"Error while sending message to Twitch: '{message}'");
        return;
      }
      if (flush)
      {
        _OutputStream.Flush();
      }

    }

    private TimeSpan? CheckThrottleStatus()
    {

      var throttleDuration = TimeSpan.FromSeconds(30);

      if (_NextReset == null)
      {
        _NextReset = DateTime.UtcNow.Add(throttleDuration);
        _CommandCount = 0;
      }
      else if (_NextReset < DateTime.UtcNow)
      {
        _NextReset = DateTime.UtcNow.Add(throttleDuration);
        _CommandCount = 0;
      }

      if (_CommandCount < MAXIMUMCOMMANDS)
      {
        _CommandCount++;
        return null;
      }

      return _NextReset.Subtract(DateTime.UtcNow);

    }


    private static bool IsWhisper(string message)
    {

      return reWhisperMessage.IsMatch(message);

    }


    public static bool HandlePong(string message, ILogger logger, Action<string> send)
    {

      if (!message.StartsWith("PING")) return false;

      logger.LogWarning("Received PING from Twitch... sending PONG");
      send($"PONG :{message.Split(':')[1]}");
      return true;

    }

    public void Dispose()
    {
      ((IDisposable)_Client).Dispose();
    }

  }

}
