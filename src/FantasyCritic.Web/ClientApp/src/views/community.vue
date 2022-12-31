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
              <a href="https://github.com/rarDevelopment" target="_blank">Robert (rardk64)</a>
              and myself (Steve Fallon). However, compared to the below bot, this one is truly official and integrated into the site itself. Here's a summary of the bot's features:
            </p>

            <h4>Setup Instructions</h4>
            <p>To set up the bot, do the following:</p>
            <ol>
              <li>Click the link above to invite the bot to your Discord. It requires all permissions that it requests.</li>
              <li>Go to the channel(s) where you want the bot to be active and use the /set-league command (see below for details). You will need the league ID from your league's URL.</li>
              <li>Configure the Game News setting to your liking using the set-game-news command (see below for details).</li>
            </ol>

            <h4>League News Announcements</h4>
            <p>
              Updates to games (configurable with the command mentioned below), league actions, bid results on Saturdays, public bids on Thursdays, publisher name changes, and more will all be
              announced real-time as they happen on the Fantasy Critic site at their appropriate times.
            </p>

            <h4>Available Bot Actions</h4>

            <h5>See League Information</h5>
            <p>
              The
              <strong>/league</strong>
              command will display the list of publishers in the league, ordered by their current rankings from first to last place. It will also display the publishers' current scores, their project
              scored, and the number of games they have.
            </p>
            <h5>See League Options</h5>
            <p>
              The
              <strong>/league-options</strong>
              command will display the options chosen for that league year, including the number of picks and counter picks, settings related to bids and drops, banned tags, and more.
            </p>
            <h5>Get a Link to the League</h5>
            <p>
              The
              <strong>/link</strong>
              command will provide a link to the league's page on the Fantasy Critic site.
            </p>

            <h5>See Publisher Information</h5>
            <p>
              The
              <strong>/publisher</strong>
              command will show details on the publisher given their publisher name or player name. This will include their picks, counter picks, current score, remaining budget, and remaining drop
              details.
            </p>

            <h5>Search for a Game</h5>
            <p>
              The
              <strong>/game</strong>
              command allows you to search for a game by its name. It will prioritize a game if it's in the league on someone's roster, but if it's not, it'll still return that game's details as it
              exists on the Fantasy Critic site.
            </p>

            <h5>View Upcoming League Releases</h5>
            <p>
              The
              <strong>/upcoming</strong>
              command displays upcoming releases for players in the league.
            </p>

            <h5>View Recent League Releases</h5>
            <p>
              The
              <strong>/recent</strong>
              command displays games that were recently released on players' rosters in the league.
            </p>

            <h5>Set Up a League in your Channel</h5>
            <p>
              The
              <strong>/set-league</strong>
              command allows you to connect your league to a Discord channel.
              <strong>Note:</strong>
              If your league is private, the user setting up the league will need to have their Discord account connected to their Fantasy Critic account on the website.
            </p>

            <h5>Remove the League in your Channel</h5>
            <p>
              The
              <strong>/remove-league</strong>
              command allows you to remove your league from a Discord channel.
            </p>

            <h5>Configure Game News Announcement Relevance</h5>
            <p>
              The
              <strong>/set-game-news</strong>
              command allows you to configure how much game news you want the bot to provide. There are three settings available for this:
            </p>
            <ul>
              <li>
                <strong>All</strong>
                - All updates to games on the site will be posted by the bot.
              </li>
              <li>
                <strong>Relevant</strong>
                - Only game updates that are relevant to your league will be announced. Relevance criteria includes games that are in your league, were previously in your league, are coming out this
                year, or are just now being delayed out of the current year.
              </li>
              <li>
                <strong>League Games Only</strong>
                - Game updates will only be announced if they are for a game that is in your league or was previously in your league.
              </li>
              <li>
                <strong>Off</strong>
                - No game news will be posted by the bot.
              </li>
            </ul>

            <h5>And more to come!</h5>
            <p>This bot will see continued development moving forward. Got an idea for a feature? Give us a shout in the official Fantasy Critic Discord.</p>
          </div>
          <hr />
          <div>
            <h3>fantasy-critic-bot</h3>
            <div>
              <a href="https://github.com/rarDevelopment/fantasy-critic-bot" target="_blank">
                <font-awesome-icon :icon="['fab', 'github']" size="lg" class="github-icon" fixed-width />
                GitHub
              </a>
            </div>
            <br />
            <p>
              Bot created by
              <a href="https://github.com/rarDevelopment" target="_blank">Robert (rardk64)</a>
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
              You can also issue commands to the bot and it will give info such as the current standings for your league. Full instructions are available on GitHub. One important note: the bot does
              not work with private leagues. Note that while this bot is still operational for now, development efforts going forward will be focused on the official bot above.
            </p>
          </div>
          <hr />
          <div>
            <h3>fcbot</h3>
            <div>
              <a href="https://github.com/drcatdoctor/fcbot" target="_blank">
                <font-awesome-icon :icon="['fab', 'github']" size="lg" class="github-icon" fixed-width />
                GitHub
              </a>
            </div>
            <br />
            <p>
              Bot created by
              <a href="https://github.com/drcatdoctor" target="_blank">drcat.</a>
              While this bot is still functional, it has not been updated in a while. Full instructions are available on GitHub, but I can't guarantee full functionality. Notably, the method described
              on GitHub to make the bot connect to a private league no longer works.
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
