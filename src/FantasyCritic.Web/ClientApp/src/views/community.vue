<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <h1>Community</h1>
      <hr />
      <div>
        <h2>Discord</h2>
        <div class="text-well">
          <p>The Discord is the primary home for the Fantasy Critic community. We have channels for:</p>
          <ul>
            <li>Finding a league</li>
            <li>Rating each other's game lineups</li>
            <li>Debating what tags a game should have</li>
            <li>Tech Support</li>
            <li>Discussing new features</li>
            <li>...and more</li>
          </ul>
          <a href="https://discord.gg/dNa7DD3" target="_blank">
            <font-awesome-icon :icon="['fab', 'discord']" size="lg" class="discord-icon" />
            Join our Server
          </a>
        </div>
      </div>
      <div>
        <h2>Subreddit</h2>
        <div class="text-well">
          <p>
            The subreddit admittedly doesn't see as much traffic as it used to, as it's been largely supplanted by our Discord. However, it may still be where you want to go if you want to post
            something very detailed like a feature proposal.
          </p>
          <a href="https://www.reddit.com/r/fantasycritic/" target="_blank">
            <font-awesome-icon :icon="['fab', 'reddit']" size="lg" class="reddit-icon" />
            r/fantasycritic
          </a>
        </div>
      </div>
      <div>
        <h2>Discord Bots</h2>
        <div class="text-well">
          <div>
            <h3>fantasy-critic-bot</h3>
            <p>
              Bot created by
              <a href="https://github.com/rarDevelopment" target="_blank">Robert</a>
              with additional development by me (Steve Fallon). The bot will send automatic updates for:
            </p>
            <ul>
              <li>Game releases</li>
              <li>Game score changes</li>
              <li>Game release date changes</li>
              <li>Public bids</li>
              <li>Trade proposals</li>
            </ul>
            <p>
              You can also issue commands to the bot and it will give info such as the current standings for your league. Full instructions are available on Github. This bot is still being worked on
              and will include more features down the line. One important note: the bot does not work with private leagues, although we may change that down the line.
            </p>
            <div>
              <a href="https://discord.com/api/oauth2/authorize?client_id=936106026695028807&permissions=139586914368&scope=bot" target="_blank">
                <font-awesome-icon :icon="['fab', 'discord']" size="lg" class="discord-icon" fixed-width />
                Add To Your Discord Server
              </a>
            </div>
            <div>
              <a href="https://github.com/rarDevelopment/fantasy-critic-bot" target="_blank">
                <font-awesome-icon :icon="['fab', 'github']" size="lg" class="github-icon" fixed-width />
                Github
              </a>
            </div>
          </div>
          <hr />
          <div>
            <h3>fcbot</h3>
            <p>
              Bot created by
              <a href="https://github.com/drcatdoctor" target="_blank">drcat.</a>
              While this bot is still functional, it has not been updated in a while. Full instructions are available on Github, but I can't guarantee full functionality. Notably, the method described
              on Github to make the bot connect to a private league no longer works.
            </p>
            <div>
              <a href="https://discord.com/api/oauth2/authorize?client_id=671814513967890459&permissions=2048&scope=bot" target="_blank">
                <font-awesome-icon :icon="['fab', 'discord']" size="lg" class="discord-icon" fixed-width />
                Add To Your Discord Server
              </a>
            </div>
            <div>
              <a href="https://github.com/drcatdoctor/fcbot" target="_blank">
                <font-awesome-icon :icon="['fab', 'github']" size="lg" class="github-icon" fixed-width />
                Github
              </a>
            </div>
          </div>
        </div>
      </div>
      <div>
        <h2>Your creations!</h2>
        <div class="text-well">Have you made something Fantasy Critic related that you think is cool? Contact me and I might just feature it here.</div>
      </div>
    </div>
  </div>
</template>
