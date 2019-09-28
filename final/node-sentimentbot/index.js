const tmi = require('tmi.js')
const superagent = require('superagent')
require('dotenv').config()

// Define configuration options
const opts = {
  identity: {
    username: process.env.BOT_USERNAME,
    password: process.env.OAUTH_TOKEN
  },
  channels: [
    process.env.CHANNEL_NAME
  ]
};

// Create a client with our options
const client = new tmi.client(opts);

// Register our event handlers (defined below)
client.on('message', onMessageHandler);
client.on('connected', onConnectedHandler);

// Connect to Twitch:
client.connect();

// Called every time a message comes in
async function onMessageHandler (target, context, msg, self) {
  if (self) { return; } // Ignore messages from the bot

  // Remove whitespace from chat message
  const message = msg.trim();
  let response = superagent.post('https://twitchcon.csharpfritz.com/sentiment')
    .send({
      SentimentText: message,
      Channel: 'nodebotanist'
    })
    .end((err, res) => {
      console.log('error', err)
      console.log('response', res.body)
      if(res.body) {
        client.say('nodebotanist', `Sentiment for message ${message} is ${res.body}`)
      }
    })

}

// Called every time the bot connects to Twitch chat
function onConnectedHandler (addr, port) {
  console.log(`* Connected to ${addr}:${port}`);
}
