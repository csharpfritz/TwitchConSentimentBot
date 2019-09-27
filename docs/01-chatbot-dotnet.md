# Building a Simple Twitch ChatBot with .NET Core 3

In this module, we will build a simple chatbot using C# and .NET Core 3 in a Worker Service project.  A worker service project allows you to run your application in a console or register it as a service to run on Windows or Linux.

You will complete the following tasks and learn how to:

  + Authenticate with Twitch chat servers
  + Identify Twitch IRC extensions 
  + Listen to and identify Twitch chat activity
  + Keep alive your bot when Twitch tests its status
  + Send messages and whispers to the Twitch chat servers
  + Rate limits for interacting with Twitch APIs

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

    "CAP REQ :twitch.tv/commands twitch.tv/membership"

This requests various additional pieces of information about chat rooms and capabilities of Twitch chat:

  + `twitch.tv/commands` enables some Twitch specific commands 
  + `twitch.tv/membership` adds information about the membership state in the chat room

Next, let's send our credentials to login by writing the following into the `_OutputStream.WriteLine()` with the following three commands:

    $"PASS oauth:{AccessToken}"
    $"NICK {BotName}"
    $"USER {BotName} 8 * :{BotName}"

This uses a little C# string interpolation to place an Access Token into the password field.  Additionally, the name of your account, the BotName, will be used as a nickname and the user account for the login.  

The values for the `AccessToken` and the `BotName` are stored in `appSettings.json` in the `Bot.Name` field and the `Bot.AccessToken` properties.  Replace the dummy values there with the name of your bot's Twitch account and the Access token (without the leading `oauth:`) for your account.

Next, we need to join the channel that our bot will listen to.  We can join the default chat room for a channel with the following command, let's add it to the last `_OutputStream.WriteLine` command:

    $"JOIN #{ChannelName}"

This joins the specified channel.  The ChannelName value is fetched from the `Bot.Channel` property in the `appSettings.json` file.

With these values entered, out bot now knows how to connect to Twitch with the account we chose and join the channel specified.

## Listening to the chat room

Chat messages from Twitch in the current room come in the raw format of `PRIVMSG #CHANNELNAME :<MESSAGE>` and private messages for our bot are sent in the format `WHISPER BOTNAME :<MESSAGE>`.  We can easily write some regular expressions to help identify these two message formats.  Add the regular expression assignments in the constructor of the Chatbot.cs file, where noted with a TODO with these values:

    reChatMessage = new Regex($@"PRIVMSG #{ChannelName} :(.*)$");
    reWhisperMessage = new Regex($@"WHISPER {BotName} :(.*)$");

We can now identify the different message types our chatbot will see.

## Sending messages to Twitch

Besides broadcasting messages to the chatroom and sending private messages, we will also need to handle the Twitch keep-alive messagge called a `PING`.  Twitch will send the following message every 5 minutes to check if a chat client is still connected:

    PING :tmi.twitch.tv

Your bot will need to respond appropriately to this `PING` with a `PONG` or Twitch will disconnect it for inactivity.  A method has already been started for you in the Chatbot.cs file called `HandlePong`. Add the following response code where indicated with the TODO:

    _OutputStream.WriteLine($"PONG :{message.Split(':')[1]}");
    _OutputStream.Flush();

This will split the incoming `PING` message, capturing the information after the colon and sending it back as the response.   

Congratulations, your bot now will stay connected to Twitch as long as your application is running.

## Sending Messages to Chat

We need to properly format messages that are sent to the Twitch chat room.  There are two methods in this class that format appropriately the IRC message for you and are called `PostMessage` and `WhisperMessage` appropriately.

In `PostMessage` add the following format for the `fullMessage` string variable:

    $":{BotName}!{BotName}@{BotName}.tmi.twitch.tv PRIVMSG #{ChannelName} :{message}";

This format indicates the Bot is sending a message on the Twitch server, privately to the recipients in the `ChannelName` channel with the `message` as the contents to be displayed.

We can format private messages, or whispers similarly in the `WhisperMessage` method's `fullMessage` variable:

    $":{BotName}!{BotName}@{BotName}.tmi.twitch.tv PRIVMSG #jtv :/w {userName} {message}"

This time, we are indicating the the Bot is sending a message on Twitch's server directly to the `userName` specified with the `message` to be sent.

Your bot now knows how to chat with the chatroom.

## Rate Limits

Finally, we need to be good Twitch citizens, and obey the Twitch Speed Limit.  Chatters are only allowed to issue commands or messages at a certain speed in the chatroom.  Failure to abide by these limits will result in a ban of your bot's account.

The Chatbot.cs file is pre-configured to be able to handle throttling those messages with a `CheckThrottleStatus` method.

The rate limits for Twitch are [defined in their docs](https://dev.twitch.tv/docs/irc/guide/#command--message-limits).  For a bot that is NOT a moderator, we can only send 20 messages per 30 seconds.  Hopefully, you will make your bot a moderator on your channel, which will grant it 100 messages per 30 seconds.

Let's set those thresholds on our bot's fields at the top of the Chatbot.cs file:

    const int MAXIMUMCOMMANDS = 100;
    TimeSpan _ThrottleDuration = TimeSpan.FromSeconds(30);

That's it!  You now have a chatbot that knows how to login, interact with Twitch chat, and stay connected.

There are methods in Chatbot_Handlers that will inspect each message that arrives and send it to a service that will perform Sentiment Analysis on the text, returning a value between 0 and 1.  Zero indicates a very negative message, and one indicates a very positive message.  The bot is configured to log this value to the local console for each message in chat.  More details about the Sentiment Analysis service can be discussed with [csharpfritz](https://twitter.com/csharpfritz)