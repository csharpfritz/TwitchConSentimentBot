# Building a Simple Twitch ChatBot with .NET Core 3

In this module, we will build a simple chatbot using C# and .NET Core 3 in a Worker Service project.  A worker service project allows you to run your application in a console or register it as a service to run on Windows or Linux.

You will complete the following tasks and learn:

  + 

The application framework has been built for you, along with the interactions with our sentiment analysis service.  We are providing this base framework so that you do not have to learn about how to build the project, and can instead focus on learning how to interact with the Twitch APIs.



Identify Chat and Whispers
      reChatMessage = new Regex($@"PRIVMSG #{ChannelName} :(.*)$");
      reWhisperMessage = new Regex($@"WHISPER {BotName} :(.*)$");


COnnect to IRC
      _Client = new TcpClient("irc.chat.twitch.tv", 80);


"CAP REQ :twitch.tv/tags twitch.tv/commands twitch.tv/membership"

$"PASS oauth:{AccessToken}"

$"NICK {BotName}"

$"USER {BotName} 8 * :{BotName}"

$"JOIN #{ChannelName}"

$"JOIN #chatrooms:{ChannelId}:{ChatroomId}"

$":{BotName}!{BotName}@{BotName}.tmi.twitch.tv PRIVMSG #chatrooms:{ChannelId}:{ChatroomId} :{message}";

$":{BotName}!{BotName}@{BotName}.tmi.twitch.tv PRIVMSG #{ChannelName} :{message}";

