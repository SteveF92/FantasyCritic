<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <h1>Discord Bot</h1>
      <hr />
      <div>
        <h2>Official Fantasy Critic Bot</h2>
        <div class="text-well">
          <div>
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
              and myself (Steve Fallon), and is integrated into the site itself. Here's a summary of the bot's features:
            </p>

            <h4>Setup Instructions</h4>
            <p>To set up the bot, do the following:</p>
            <ol>
              <li>
                First, I
                <em>highly</em>
                recommend
                <router-link :to="{ hash: '#link-discord-account' }">linking your Discord account</router-link>
                to this site if you haven't already. This is safe and will make for easier use of the bot.
              </li>
              <li>Click the link above to invite the bot to your Discord server. It requires all permissions that it requests.</li>
              <li>
                Set up the bot in your Discord server's preferred channel(s). You can do this in any of the following ways (see
                <a href="#how-to-use">How To Use The Bot</a>
                below for more info):
                <ul>
                  <li>
                    For
                    <router-link :to="{ hash: '#league-updates' }">League Updates</router-link>
                    use the
                    <code>/set-league</code>
                    command in the appropriate channel(s).
                  </li>
                  <li>
                    For
                    <router-link :to="{ hash: '#game-news' }">Game News</router-link>
                    use the
                    <code>/set-game-news</code>
                    command in the appropriate channel(s).
                  </li>
                </ul>
              </li>
            </ol>

            <h4 id="link-discord-account">Linking Your Discord Account to Fantasy Critic</h4>
            <p>
              <strong>How?</strong>
              To link your Discord account,
              <a href="/Account/Manage/ExternalLogins">click here</a>
              , click Link Discord Account, and click Authorize.
            </p>
            <p>
              <strong>Why?</strong>
              Linking your Discord account to Fantasy Critic allows the bot to know your Fantasy Critic account when using the bot from your Discord channel. Doing this allows easier use of commands,
              and will allow us to give you more information directly in Discord in certain functions.
            </p>

            <h4 id="league-updates">League Updates</h4>
            <p>League actions, bid results on Saturdays, public bids on Thursdays, publisher score updates, and more will post real-time as they happen.</p>

            <h4 id="game-news">Game News</h4>
            <p>Publisher score updates, rank changes, bid results, and more will post real-time as they happen. This does not require a league to be set up,</p>

            <collapseCard>
              <div id="how-to-use" slot="header">How to Use The Bot</div>
              <div slot="body">
                <h3>Available Bot Commands</h3>
                <ul>
                  <li><router-link :to="{ hash: '#league' }">See League Information</router-link></li>
                  <li><router-link :to="{ hash: '#league-options' }">See League Options</router-link></li>
                  <li><router-link :to="{ hash: '#link' }">Get a Link to the League</router-link></li>
                  <li><router-link :to="{ hash: '#publisher' }">See Publisher Information</router-link></li>
                  <li><router-link :to="{ hash: '#game' }">Search for a Game</router-link></li>
                  <li><router-link :to="{ hash: '#upcoming' }">View Upcoming League Releases</router-link></li>
                  <li><router-link :to="{ hash: '#recent' }">View Recent League Releases</router-link></li>
                  <li><router-link :to="{ hash: '#public-bids' }">View Public Bids for the Current Week</router-link></li>
                  <li><router-link :to="{ hash: '#special-auctions' }">View Current Special Auctions</router-link></li>
                  <li><router-link :to="{ hash: '#set-league' }">Set Up a League in your Channel</router-link></li>
                  <li><router-link :to="{ hash: '#remove-league' }">Remove the League in your Channel</router-link></li>
                  <li><router-link :to="{ hash: '#set-bid-alert-role' }">Set a Role to Alert for Bid Information</router-link></li>
                  <li><router-link :to="{ hash: '#set-game-news' }">Configure Game News Announcements</router-link></li>
                  <li><router-link :to="{ hash: '#view-settings' }">View Bot Settings</router-link></li>
                </ul>
                <hr />
                <div class="discord-command-explanation">
                  <h5 id="league">See League Information</h5>
                  <p>
                    <span class="command-label">Command:</span>
                    <code>/league</code>
                  </p>
                  <p>
                    <span class="discord-param-text">
                      <span class="param-label">Parameters:</span>
                      <p>
                        <code>year:</code>
                        [optional] The year for the league you'd like to search. Defaults to earliest active year.
                      </p>
                    </span>
                  </p>
                  <span>
                    Displays the list of publishers in the league, ordered by their current rankings from first to last place. It will also display the publishers' current scores, their project
                    scored, and the number of games they have.
                  </span>
                </div>
                <hr />
                <div class="discord-command-explanation">
                  <h5 id="league-options">See League Options</h5>
                  <p>
                    <span class="command-label">Command:</span>
                    <code>/league-options</code>
                  </p>
                  <p>
                    <span class="discord-param-text">
                      <span class="param-label">Parameters:</span>
                      <p>
                        <code>year:</code>
                        [optional] The year for the league for which you'd like to see options. Defaults to earliest active year.
                      </p>
                    </span>
                  </p>
                  <span>Displays the options chosen for that league year, including the number of picks and counter picks, settings related to bids and drops, banned tags, and more.</span>
                </div>
                <hr />
                <div class="discord-command-explanation">
                  <h5 id="link">Get a Link to the League</h5>
                  <p>
                    <span class="command-label">Command:</span>
                    <code>/link</code>
                  </p>
                  <p>
                    <span class="discord-param-text">
                      <span class="param-label">Parameters:</span>
                      <p>
                        <code>year:</code>
                        [optional] The year for the league. Defaults to earliest active year.
                      </p>
                    </span>
                  </p>
                  <span>Provides a link to the league's page on the Fantasy Critic site.</span>
                </div>
                <hr />
                <div class="discord-command-explanation">
                  <h5 id="publisher">See Publisher Information</h5>
                  <p>
                    <span class="command-label">Command:</span>
                    <code>/publisher</code>
                  </p>
                  <p>
                    <span class="discord-param-text">
                      <span class="param-label">Parameters:</span>
                      <p class="parameter-explanation">
                        <code>publisher_or_player_name:</code>
                        The publisher name or player name for the user you're searching for.
                        <br />
                        <strong>NOTE:</strong>
                        If you have your account linked to Discord, you do not need to include your name here to show your own publisher information. Simply entering
                        <code>/publisher</code>
                        will suffice.
                      </p>
                      <p class="parameter-explanation">
                        <code>year:</code>
                        [optional] The year for the league. Defaults to earliest active year.
                      </p>
                    </span>
                  </p>
                  <span>
                    Shows details on the publisher given their publisher name or player name. This will include their picks, counter picks, current score, remaining budget, and remaining drop details.
                  </span>
                </div>
                <hr />
                <div class="discord-command-explanation">
                  <h5 id="game">Search for a Game</h5>
                  <p>
                    <span class="command-label">Command:</span>
                    <code>/game</code>
                  </p>
                  <p>
                    <span class="discord-param-text">
                      <span class="param-label">Parameters:</span>
                      <p class="parameter-explanation">
                        <code>game_name:</code>
                        The game name that you're searching for. You only need to enter a portion of the name.
                      </p>
                      <p class="parameter-explanation">
                        <code>year:</code>
                        [optional] The year for the league. Defaults to earliest active year.
                      </p>
                    </span>
                  </p>
                  <span>
                    Allows you to search for a game by its name. It will prioritize a game if it's in the league on someone's roster, but if it's not, it'll still return that game's details as it
                    exists on the Fantasy Critic site.
                  </span>
                </div>
                <hr />
                <div class="discord-command-explanation">
                  <h5 id="upcoming">View Upcoming League Releases</h5>
                  <p>
                    <span class="command-label">Command:</span>
                    <code>/upcoming</code>
                  </p>
                  <span>Displays upcoming releases for players in the league.</span>
                </div>
                <hr />
                <div class="discord-command-explanation">
                  <h5 id="recent">View Recent League Releases</h5>
                  <p>
                    <span class="command-label">Command:</span>
                    <code>/recent</code>
                  </p>
                  <span>Displays games that were recently released for players in the league.</span>
                </div>
                <hr />
                <div class="discord-command-explanation">
                  <h5 id="public-bids">View Public Bids for the Current Week</h5>
                  <p>
                    <span class="command-label">Command:</span>
                    <code>/public-bids</code>
                  </p>
                  <p>
                    <span class="discord-param-text">
                      <span class="param-label">Parameters:</span>
                      <p class="parameter-explanation">
                        <code>year:</code>
                        [optional] The year for the league. Defaults to earliest active year.
                      </p>
                    </span>
                  </p>
                  <span>Displays the current public bids for that week. This only applies to leagues that aren't using Secret Bidding, and only during the Public Bidding window.</span>
                </div>
                <hr />
                <div class="discord-command-explanation">
                  <h5 id="special-auctions">View Current Special Auctions</h5>
                  <p>
                    <span class="command-label">Command:</span>
                    <code>/special-auctions</code>
                  </p>
                  <p>
                    <span class="discord-param-text">
                      <span class="param-label">Parameters:</span>
                      <p class="parameter-explanation">
                        <code>year:</code>
                        [optional] The year for the league. Defaults to earliest active year.
                      </p>
                    </span>
                  </p>
                  <span>Displays the currently running special auctions, if there are any.</span>
                </div>
                <hr />
                <div class="discord-command-explanation">
                  <h5 id="set-league">Set Up a League in your Channel</h5>
                  <p>
                    <span class="command-label">Command:</span>
                    <code>/set-league</code>
                  </p>
                  <p>
                    <span class="discord-param-text">
                      <span class="param-label">Parameters:</span>
                      <p class="parameter-explanation">
                        <code>league_id:</code>
                        The ID for your league. You can find it in the URL for your league.
                        <br />
                        <strong>Example:</strong>
                        https://www.fantasycritic.games/league/LEAGUE_ID_HERE/2023
                      </p>
                    </span>
                  </p>
                  <span>
                    Allows you to connect your league to a Discord channel.
                    <br />
                    <strong>Note:</strong>
                    If your league is private, the user setting up the league will need to have their Discord account connected to their Fantasy Critic account on the website.
                  </span>
                </div>
                <hr />
                <div class="discord-command-explanation">
                  <h5 id="remove-league">Remove the League in your Channel</h5>
                  <p>
                    <span class="command-label">Command:</span>
                    <code>/remove-league</code>
                  </p>
                  <span>Allows you to remove your league from a Discord channel.</span>
                </div>
                <hr />
                <div class="discord-command-explanation">
                  <h5 id="set-bid-alert-role">Set a Role to Alert for Bid Information</h5>
                  <p>
                    <span class="command-label">Command:</span>
                    <code>/set-bid-alert-role</code>
                  </p>
                  <p>
                    <span class="discord-param-text">
                      <span class="param-label">Parameters:</span>
                      <p class="parameter-explanation">
                        <code>role:</code>
                        The role to mention when bid information is posted.
                      </p>
                    </span>
                  </p>
                  <span>
                    Allows you to set a role to tag when bid updates are posted. This includes the Saturday night bid results, as well as the Thursday night public bid updates (if your league using
                    Public Bidding). You'll need to create a role in your Discord with the relevant channel members assigned to it.
                  </span>
                </div>
                <hr />
                <div class="discord-command-explanation">
                  <h5 id="set-game-news">Configure Game News Announcements</h5>
                  <p>
                    <span class="command-label">Command:</span>
                    <code>/set-game-news</code>
                  </p>
                  <p>
                    <span class="discord-param-text">
                      <span class="param-label">Parameters:</span>
                      <p class="parameter-explanation">
                        <code>setting:</code>
                        The game news setting for games to include in game news announcements. You can choose from the following settings:
                      </p>
                      <ul>
                        <li>
                          <strong>Recommended (Default)</strong>
                          - Game News will be set to Might Release (see below).
                        </li>
                        <li>
                          <strong>All</strong>
                          - All updates to games on the site will be posted by the bot.
                        </li>
                        <li>
                          <strong>Might Release In Year</strong>
                          - Only games that are considered "Might Release" will be included in game news announcements. This includes anything slated for the current year, as well as "TBD" release
                          dates which might release in the current year. If you choose this, you will also be asked if you'd like to hear about games in your league no matter what.
                        </li>
                        <li>
                          <strong>Will Release In Year</strong>
                          - Only games that are considered "Will Release" will be included in game news announcements. This includes anything slated for the current year. If you choose this, you will
                          also be asked if you'd like to hear about games in your league no matter what.
                        </li>
                        <li>
                          <strong>League Games Only</strong>
                          - Only games that are on league member's rosters will be included. If you choose this, you will also be asked if you'd also like to hear about "notable misses", which are
                          games that scored well but were not picked up by anybody in the league.
                        </li>
                        <li>
                          <strong>Off</strong>
                          - No game news will be posted by the bot.
                        </li>
                      </ul>
                      <p class="parameter-explanation">
                        <code>skip_unannounced:</code>
                        Whether or not to skip games tagged as "Unannounced" in game news announcements.
                      </p>
                    </span>
                  </p>
                  <span>Allows you to configure how much game news you want the bot to provide.</span>
                </div>
                <hr />
                <div class="discord-command-explanation">
                  <h5 id="view-settings">View Bot Settings</h5>
                  <p>
                    <span class="command-label">Command:</span>
                    <code>/view-settings</code>
                  </p>
                  <span>Displays the current settings for the bot.</span>
                </div>
                <hr />
                <div>
                  <h5>And more to come!</h5>
                  <p>This bot will see continued development moving forward. Got an idea for a feature? Give us a shout in the official Fantasy Critic Discord.</p>
                </div>
              </div>
            </collapseCard>
          </div>
        </div>
        <hr />
        <div>
          <h2>Unofficial Discord Bots</h2>
          <div class="text-well">
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
            <hr />
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
    </div>
  </div>
</template>

<script>
import CollapseCard from '@/components/collapseCard';

export default {
  components: {
    CollapseCard
  },
  data() {
    return {};
  },
  methods: {
    getCollapseID() {
      let thisID = this.lastID;
      this.lastID = this.lastID + 1;
      return thisID;
    }
  }
};
</script>
<style scoped>
.command-label {
  margin-right: 3px;
  font-weight: bold;
}
.param-label {
  font-weight: bold;
}
.discord-command-explanation {
  margin-top: 20px;
  margin-bottom: 10px;
}
.parameter-explanation {
  margin-top: 10px;
}
</style>
