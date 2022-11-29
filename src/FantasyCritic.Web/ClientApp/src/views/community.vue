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
            <h3>Official Fantasy Critic Bot</h3>
            <div>
              <a href="https://discord.com/oauth2/authorize?client_id=908453581110280232&scope=bot&permissions=2147502080" target="_blank">
                <font-awesome-icon :icon="['fab', 'discord']" size="lg" class="discord-icon" fixed-width />
                Add To Your Discord Server
              </a>
            </div>
            <br />
            <p>
              New for the 2023 season! This bot was co-developed by
              <a href="https://github.com/rarDevelopment" target="_blank">Robert</a>
              and myself (Steve Fallon). However, compared to the below bot, this one is truly official and integrated into site itself. Here's a summary of new features above and beyond any of the
              old bots:
            </p>

            <h4>Game Command</h4>
            <p>
              The /game command previously would only return a game if someone in the league had it. The new /game command will prioritize a game if it's in the league on someone's roster, but if it's
              not, it'll still return that game's details as it exists on the FC website.
            </p>

            <h4>Recent Command</h4>
            <p>
              Previously, the /upcoming command would list out upcoming releases for players in the league. The new /recent command does the opposite - displays games that were recently released for
              players in the league.
            </p>

            <h4>Private Leagues</h4>
            <p>
              Private leagues can now be set up with the bot. This was previously unavailable due to the bot being external, but with the new bot being integrated directly into the website, this is
              much easier. Note: The user setting up the league will need to have their Discord account connected to their Fantasy Critic account on the website.
            </p>

            <h4>Update Announcements</h4>
            <p>
              Previously, updates such as score updates, game detail changes, and bid/drop results on Saturday nights would be checked on a predefined schedule and posted when that check ran. Now,
              however, those updates will be triggered directly by actions on the site. Bid results will be announced as soon as they're run, game and publisher score changes as soon as they're
              applied, and more.
            </p>

            <h4>Enable/Disable Game News Announcements</h4>
            <p>You can now choose whether to turn game news announcements on or off, or you can choose to only recieve updates to games that are relevant to your league.</p>

            <h4>Formatting Improvements</h4>
            <p>
              Most of the formatting of the bot's messages has been kept the same, but there are some that have been updated. You'll notice your user icon in the footer of messages where the bot
              responds to you (where this applies), and some messages like score updates have been adjusted to account for the new way they're summarized and posted.
            </p>
          </div>
          <hr />
          <div>
            <h3>fantasy-critic-bot</h3>
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
            <br />
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
          </div>
          <hr />
          <div>
            <h3>fcbot</h3>
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
            <br />
            <p>
              Bot created by
              <a href="https://github.com/drcatdoctor" target="_blank">drcat.</a>
              While this bot is still functional, it has not been updated in a while. Full instructions are available on Github, but I can't guarantee full functionality. Notably, the method described
              on Github to make the bot connect to a private league no longer works.
            </p>
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
