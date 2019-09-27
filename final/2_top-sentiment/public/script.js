(() => {
  const CLIENT_ID = 'f5tasfbltuqwuuhbbewri3p2mgmm54'

  //fetch top sentiments from Fritz's endpoint
  //let topSentiments = await fetch()

  // for now here's some dummy data
  let topSentiments = [{
    UserName: 'nodebotanist',
    Sentiment: 1.0
  }, {
    UserName: 'noopkat',
    Sentiment: 0.9
  }, {
    UserName: 'talk2megooseman',
    Sentiment: 0.85
  },{
    UserName: 'luckynos7evin',
    Sentiment: 0.8
  },{
    UserName: 'csharpfritz',
    Sentiment: 0.7
  }]

  // cycle through each
  topSentiments.forEach(async function(user){
    // get their profile picture
    let profilePicture = await fetch(`https://api.twitch.tv/kraken/users?login=${user.UserName}`, {
      method: 'GET',
      headers: {
        'Client-ID': CLIENT_ID,
        Accept: 'application/vnd.twitchtv.v5+json'
      }
    })
    profilePicture = await profilePicture.json()
    profilePicture = profilePicture.users[0].logo
    // create li
    const liHTML = `<img src="${profilePicture}" /><p>${user.UserName} -- ${user.Sentiment}</p>`
    let liElement = document.createElement('li')
    liElement.innerHTML = liHTML
    // inject li
    document.querySelector('.js-top-sentiment').appendChild(liElement)
  })
})()