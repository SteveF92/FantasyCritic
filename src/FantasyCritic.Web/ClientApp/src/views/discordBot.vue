<template>
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
            The official Fantasy Critic bot was co-developed by
            <a href="https://github.com/rarDevelopment" target="_blank">Robert (rardk64)</a>
            and myself (Steve Fallon), and is integrated into the site itself.
          </p>

          <h4>Setup Instructions</h4>
          <p>To set up the bot, do the following:</p>
          <ol>
            <li>
              First, we
              <em>highly</em>
              recommend
              <router-link :to="{ hash: '#link-discord-account' }">linking your Discord account</router-link>
              to this site if you haven't already. This is safe and will make for easier use of the bot.
            </li>
            <li>Click the link above to invite the bot to your Discord server. It requires all permissions that it requests.</li>
            <li>
              Set up the bot in your Discord server's preferred channel(s). You can do this in any of the following ways (see
              <router-link :to="{ hash: '#how-to-use' }" @click.native="showCollapse('how-to-use')">How To Use The Bot</router-link>
              below for more info):
              <ul>
                <li>
                  For
                  <router-link :to="{ hash: '#set-league' }" @click.native="showCollapse('set-league')">League Updates</router-link>
                  use the
                  <code>/set-league</code>
                  command in the appropriate channel(s).
                </li>
                <li>
                  For
                  <router-link :to="{ hash: '#game-news-settings' }" @click.native="showCollapse('game-news-settings')">Game News</router-link>
                  use the
                  <code>/game-news-settings</code>
                  command in the appropriate channel(s).
                </li>
                <li>You can also use both commands in the same channel to configure your channel to receive both types of updates!</li>
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

          <h4>League Updates</h4>
          <p>The following information will be posted real-time as it happens on the website:</p>
          <ul>
            <li>League Actions</li>
            <li>Publisher Score Updates</li>
            <li>Publisher League Registration Notifications</li>
            <li>Publisher Updates (name changes, league manager manual updates)</li>
            <li>Drop Results (on Saturdays)</li>
            <li>Bid Results (on Saturdays)</li>
            <li>Trade Proposals and Updates</li>
            <li>League Manager Messages</li>
            <li>Public Bid Games (on Thursdays)</li>
            <li>Special Auction Information</li>
            <li>Final Standings When The Year Ends</li>
          </ul>

          <h4>Game News</h4>
          <p>The following information will be posted real-time as it happens on the website:</p>
          <ul>
            <li>New Games</li>
            <li>Game Changes (renames, added details, tag changes)</li>
            <li>Game Critic Score Updates</li>
            <li>Game Score Updates</li>
            <li>Game Release Updates</li>
          </ul>

          <collapseCard ref="discordBotInfoCollapseRef">
            <template #header>
              <div id="how-to-use">How to Use The Bot</div>
            </template>

            <template #body>
              <h3>Available Bot Commands</h3>
              <ul>
                <li><router-link :to="{ hash: '#league' }">See League Information</router-link></li>
                <li><router-link :to="{ hash: '#league-options' }">See League Options</router-link></li>
                <li><router-link :to="{ hash: '#link-to-league' }">Get a Link to the League</router-link></li>
                <li><router-link :to="{ hash: '#conference' }">See Conference Information</router-link></li>
                <li><router-link :to="{ hash: '#link-to-conference' }">Get a Link to the Conference</router-link></li>
                <li><router-link :to="{ hash: '#publisher' }">See Publisher Information</router-link></li>
                <li><router-link :to="{ hash: '#game' }">Search for a Game</router-link></li>
                <li><router-link :to="{ hash: '#game-news' }">View Upcoming or Recent League Releases</router-link></li>
                <li><router-link :to="{ hash: '#public-bids' }">View Public Bids for the Current Week</router-link></li>
                <li><router-link :to="{ hash: '#special-auctions' }">View Current Special Auctions</router-link></li>
                <li><router-link :to="{ hash: '#trending' }">View Trending Bids and Drops</router-link></li>
                <li><router-link :to="{ hash: '#set-league' }">Set Up a League in your Channel</router-link></li>
                <li><router-link :to="{ hash: '#remove-league' }">Remove the League in your Channel</router-link></li>
                <li><router-link :to="{ hash: '#set-conference' }">Set Up a Conference in your Channel</router-link></li>
                <li><router-link :to="{ hash: '#remove-conference' }">Remove the Conference in your Channel</router-link></li>
                <li><router-link :to="{ hash: '#set-bid-alert-role' }">Set a Role to Alert for Bid Information</router-link></li>
                <li><router-link :to="{ hash: '#game-news-settings' }">Configure Game News Announcements</router-link></li>
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
                      [optional] The year for the league. Defaults to earliest active year.
                    </p>
                  </span>
                </p>
                <span>
                  Displays the list of publishers in the league, ordered by their current rankings from first to last place. It will also display the publishers' current scores, their projected
                  scores, and the number of games they have.
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
                      [optional] The year for the league. Defaults to earliest active year.
                    </p>
                  </span>
                </p>
                <span>Displays the options chosen for that league year, including the number of picks and counter picks, settings related to bids and drops, banned tags, and more.</span>
              </div>
              <hr />
              <div class="discord-command-explanation">
                <h5 id="link-to-league">Get a Link to the League</h5>
                <p>
                  <span class="command-label">Command:</span>
                  <code>/link-to-league</code>
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
                <h5 id="conference">See Conference Information</h5>
                <p>
                  <span class="command-label">Command:</span>
                  <code>/conference</code>
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
                <span>
                  Displays the list of all publishers in every league in the conference, ordered by their current rankings from first to last place conference-wide. It will also display the
                  publishers' current scores and their projected scores.
                </span>
              </div>
              <hr />
              <div class="discord-command-explanation">
                <h5 id="link-to-conference">Get a Link to the Conference</h5>
                <p>
                  <span class="command-label">Command:</span>
                  <code>/link-to-conference</code>
                </p>
                <p>
                  <span class="discord-param-text">
                    <span class="param-label">Parameters:</span>
                    <p>
                      <code>year:</code>
                      [optional] The year for the conference. Defaults to earliest active year.
                    </p>
                  </span>
                </p>
                <span>Provides a link to the conference's page on the Fantasy Critic site.</span>
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
                  Allows you to search for a game by its name. It will prioritize a game if it's in the league on someone's roster, but if it's not, it'll still return that game's details as it exists
                  on the Fantasy Critic site.
                </span>
              </div>
              <hr />
              <div class="discord-command-explanation">
                <h5 id="game-news">View Upcoming or Recent League Releases</h5>
                <p>
                  <span class="command-label">Command:</span>
                  <code>/game-news</code>
                </p>
                <span>
                  Displays upcoming or recent releases for players in the league. You can also use this command in a Direct Message to the bot and it will display all of your upcoming or recent
                  releases across all of your leagues. NOTE: DMing the bot will require that your Discord account be linked to your Fantasy Critic account.
                </span>
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
                <h5 id="trending">View Trending Bids and Drops</h5>
                <p>
                  <span class="command-label">Command:</span>
                  <code>/trending</code>
                </p>
                <p>
                  <span class="discord-param-text">
                    <span class="param-label">Parameters:</span>
                    <p class="parameter-explanation">
                      <code>trending_topic:</code>
                      The type of trending topic you would like to see. You can choose from the following options:
                    </p>
                    <ul>
                      <li>
                        <strong>Bids</strong>
                        - The top bids on the site from the most recent processing day.
                      </li>
                      <li>
                        <strong>Counter Picks</strong>
                        - The top counter picks on the site from the most recent processing day.
                      </li>
                      <li>
                        <strong>Drops</strong>
                        - The top drops on the site from the most recent processing day.
                      </li>
                    </ul>
                  </span>
                </p>
                <span>Displays trending bids, drops, and counter picks for the most recent bid/drop processing day.</span>
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
                      The ID for your league. You can get it by clicking the
                      <font-awesome-icon :icon="['far', 'copy']" />
                      icon next to your league name. You can also find it in the URL for your league.
                      <br />
                      <strong>Example:</strong>
                      https://www.fantasycritic.games/league/LEAGUE_ID_HERE/YEAR
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
                <h5 id="set-conference">Set Up a Conference in your Channel</h5>
                <p>
                  <span class="command-label">Command:</span>
                  <code>/set-conference</code>
                </p>
                <p>
                  <span class="discord-param-text">
                    <span class="param-label">Parameters:</span>
                    <p class="parameter-explanation">
                      <code>conference_id:</code>
                      The ID for your conference. You can get it by clicking the
                      <font-awesome-icon :icon="['far', 'copy']" />
                      icon next to your conference name. You can also find it in the URL for your conference.
                      <br />
                      <strong>Example:</strong>
                      https://www.fantasycritic.games/conference/CONFERENCE_ID_HERE/YEAR
                    </p>
                  </span>
                </p>
                <span>Allows you to connect your conference to a Discord channel.</span>
              </div>
              <hr />
              <div class="discord-command-explanation">
                <h5 id="remove-conference">Remove the Conference in your Channel</h5>
                <p>
                  <span class="command-label">Command:</span>
                  <code>/remove-conference</code>
                </p>
                <span>Allows you to remove your conference from a Discord channel.</span>
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
                <h5 id="game-news-settings">Configure Game News Announcements</h5>
                <p>
                  <span class="command-label">Command:</span>
                  <code>/game-news-settings</code>
                </p>
                <span>
                  Allows you to choose exactly what types of game news you want to be notified about. This command provides an interface, complete with buttons, allowing you to fine to your desired
                  settings. The options you can choose from include:
                  <ul>
                    <li>
                      <strong>League News Settings</strong>
                      - Tells the bot to always notify you about games that are picked in your league or eligible in your league. - There is also the "Notable Miss" setting which allows you to tell
                      the bot to notify you about games that have scored very well that your league missed. You can choose between being notified about the game when it first gets a score, or any
                      updates about that game's score.
                    </li>
                    <li>
                      <strong>Game Release Settings</strong>
                      - Tells the bot whether or not to notify you about games based on whether or not they are confirmed to release this year and other similiar filters.
                    </li>
                    <li>
                      <strong>Game News Update Settings</strong>
                      - Tells the bot whether or not to notify you when a game is edited in the master database or when the game has a change in score.
                    </li>
                    <li>
                      <strong>Skipped Tags Settings</strong>
                      - Tells the bot to never notify you about a game with a specific tag. This is most commonly used to tell you to never notify you about an unnannounced game being added to the
                      database, for example.
                    </li>
                  </ul>
                </span>
              </div>
              <hr />
              <div>
                <h5>And more to come!</h5>
                <p>This bot will see continued development moving forward. Got an idea for a feature? Give us a shout in the official Fantasy Critic Discord.</p>
              </div>
            </template>
          </collapseCard>
        </div>
      </div>
      <hr />
      <div>
        <h2>Unofficial Discord Bots</h2>
        <div class="text-well">
          <h3><em>Note! These bots are here for historical purposes. Please use the official bot above!</em></h3>
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
            with additional development by me (Steve Fallon). The bot would send automatic updates for:
          </p>
          <ul>
            <li>Game releases</li>
            <li>Game score changes</li>
            <li>Game release date changes</li>
            <li>Public bids</li>
            <li>Trade proposals</li>
          </ul>
          <p>
            This was also the first Fantasy Critic bot to use Discord's slash commands feature, and working on this together is what led us to decide to take on the task of making the official bot.
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
            The original Fantasy Critic Bot, created by
            <a href="https://github.com/drcatdoctor" target="_blank">drcat.</a>
            This bot has not been updated in a while and does not function anymore. However, it's an essential part of Fantasy Critic's history and was the root inspiration for the other bots.
          </p>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import CollapseCard from '@/components/collapseCard.vue';

export default {
  components: {
    CollapseCard
  },
  data() {
    return {};
  },
  mounted() {
    if (this.$route.hash) {
      this.showCollapse(this.$route.hash);
    }
  },
  methods: {
    showCollapse(hash) {
      this.$refs.discordBotInfoCollapseRef.show();
      setTimeout(() => {
        this.$router.push({ hash: hash });
      }, 250);
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
