# Building a Simple Twitch ChatBot with .NET Core 3

In this module, we will build a simple chatbot using C# and .NET Core 3 in a Worker Service project.  A worker service project allows you to run your application in a console or register it as a service to run on Windows or Linux.

You will complete the following tasks and learn how to:

  + Authenticate with Twitch chat servers
  + Identify Twitch IRC extensions 
  + Send messages and whispers to the Twitch chat servers
  + Rate limits for interacting with Twitch APIs
  + Keep alive your bot when Twitch tests its status

The application framework has been built for you, along with the interactions with our sentiment analysis service.  We are providing this base framework so that you do not have to learn about how to build the project, and can instead focus on learning how to interact with the Twitch APIs.  

You will be working with the starter code in [1_chatbot/dotnet_core_3](/1_chatbot/dotnet_core_3).  Open this folder in your editor to proceed.

## Authentication and Permissions

Authentication with Twitch services is a complex topic and can get very confusing when discussing OpenID, OAuth, Credentials, and Code Flows.  There is a very thorough [document on the Twitch developer site](https://dev.twitch.tv/docs/authentication/) about the various forms of authentication and permissions scopes you can request.  We are going to simplify this interaction significantly for the purposes of this module.

Our chatbot will impersonate a user, and needs an ID Token to login with a password to the chat server.  You could go through an authentication process in your application that requires a user to login and authorize the chatbot to interact on their behalf.  We will shortcut this, and use the [Twitch Token Generator](https://twitchtokengenerator.com/) provided by [SwiftySpiffy](https://github.com/swiftyspiffy).  You can also use the [Twitch Chat Password Generator](https://twitchapps.com/tmi/) to generate an Access Token.

This website will allow you to login, choose that you are creating a ChatBot token, and give you two values: an Access Token and a Refresh Token.  The Access Token is effectively your 'Password' and the Refresh Token allows you to request a new Access Token when the Access Token expires.  For the purposes of this workshop, we only need the Access Token.  

Copy the access token value into a safe place, we will need it for our configuration.

Also, the website allows you to choose many different scopes or permissions for your chat bot to have access to on the Twitch platform.  For access to chat, we only need the chat_login permission.  It is recommended that you grant your application the LEAST number of permissions it requires to operate.

## Twitch IRC Extensions and Login

Twitch's chat system is a modified version of the long-standing IRC application.  The extensions provided by Twitch allow for the interactions we know and love, like user badges and cheers.

The ChatBot.cs class contains our interactions with Twitch.  Open that file and let's start interacting with the `Start` method.  This method will be called when the bot starts, and needs to know how to connect to the chat servers.

Add to the top of this method the following line to connect to chat:

    _Client = new TcpClient("irc.chat.twitch.tv", 80);

This points a TCP Client at the Twitch IRC chat server and the following lines in the method attach .NET stream objects to that client so that they can listen for messages and send messages appropriately.

Further down in the method, you will see the loop that listens for messages on the InputStream, and in other methods you will see how the OutputStream sends messages to Twitch.

Next, let's write the Login method to grant our bot access to the platform.  We need to send four messages to being this connection.  Write these four strings into the `_OutputStream.WriteLine();` methods in the Login method:

    "CAP REQ :twitch.tv/tags twitch.tv/commands twitch.tv/membership"

This defines


$"PASS oauth:{AccessToken}"

$"NICK {BotName}"

$"USER {BotName} 8 * :{BotName}"

$"JOIN #{ChannelName}"

$"JOIN #chatrooms:{ChannelId}:{ChatroomId}"

Identify Chat and Whispers
      reChatMessage = new Regex($@"PRIVMSG #{ChannelName} :(.*)$");
      reWhisperMessage = new Regex($@"WHISPER {BotName} :(.*)$");


$":{BotName}!{BotName}@{BotName}.tmi.twitch.tv PRIVMSG #chatrooms:{ChannelId}:{ChatroomId} :{message}";

$":{BotName}!{BotName}@{BotName}.tmi.twitch.tv PRIVMSG #{ChannelName} :{message}";

$":{BotName}!{BotName}@{BotName}.tmi.twitch.tv PRIVMSG #jtv :/w {userName} {message}"

      send($"PONG :{message.Split(':')[1]}");
