# Writing a chatbot in Node.JS

## Getting set up

In your terminal, create a new npm module by running

```
$ mkdir node-twitch-chatbot
$ cd node-twitch-chatbot
$ npm init -y
```

Then, install dependencies

```
$ npm i tmi.js dotenv superagent
```

[tmi.js]([https://github.com/tmijs/tmi.js) is the Twitch Messaging Interface library, which handles a lot of the grunt work for us. [dotenv](https://github.com/motdotla/dotenv) allows us to use a `.env` file to contain our super-secret oauth token and other bits of data. [superagent](https://github.com/visionmedia/superagent) helps with the process of making our HTTP requests to the sentiment analysis API.

Next, we'll create a `.env` file with the following information:

```
BOT_USERNAME="(your username)"
OAUTH_TOKEN="(your oauth token)"
CHANNEL_NAME="(your username)"
```

Check out [this tutorial](./00-get-started) if you need help getting your OAuth key.

Finally, create an `index.js` file to code in:

```
$ touch index.js
```

Now we're ready to code! 

## Coding the bot

First, we'll bring in our dependencies:

```javascript
const tmi = require('tmi.js')
const superagent = require('superagent')
require('dotenv').config()
```

Next, we'll set up the `options` object to create a Twitch chat client:

```javascript
const opts = {
  identity: {
    username: process.env.BOT_USERNAME,
    password: process.env.OAUTH_TOKEN
  },
  channels: [
    process.env.CHANNEL_NAME
  ]
};
```

Now, let's create our client:

```javascript
const client = new tmi.client(opts);
```

Next, we need to establish event handlers for connect and message events:

```javascript
client.on('message', onMessageHandler);
client.on('connected', onConnectedHandler);
```

Then, we'll tell our client to connect to Twitch IRC:

```javascript
client.connect();
```

Next, we'll write our handler for the message event. This handler will need to:

1. strip the message of extra whitespace
1. Send the message body off to the sentiment analysis API
1. Handle the response from the sentiment analysis API
1. Send a message to chat with the result

```javascript
async function onMessageHandler (target, context, msg, self) {
  if (self) { return; } // Ignore messages from the bot

  // Remove whitespace from chat message
  const message = msg.trim();
  let response = superagent.post('https://twitchcon.csharpfritz.com/sentiment')
    .send({
      SentimentText: message,
      Channel: 'nodebotanist'
    })
    })
    .end((err, res) => {
      console.log('error', err)
      console.log('response', res.body)
      if(res.body) {
        client.say('nodebotanist', `Sentiment for message ${message} is ${res.body}`)
      }
    })
}
```

Finally, let's add our handler for connect, so we know the bot has connected to Twitch IRC:

```javascript
function onConnectedHandler (addr, port) {
  console.log(`* Connected to ${addr}:${port}`);
}
```

And you're ready! Run it with 

```
$ node index.js
```

And try typing some messages into your chat!

![](../../final/node-sentiment/TwitchChatSentimentBot.PNG)

