<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <h1>Frequently Asked Questions</h1>
      <hr />

      <h2>Contents</h2>
      <ul>
        <li><a href="#general">General</a></li>
        <li><a href="#league-creation">League Creation</a></li>
        <li><a href="#drafting">Drafting</a></li>
        <li><a href="#bidding-system">Bidding System</a></li>
        <li><a href="#dropping-games">Dropping Games</a></li>
        <li><a href="#scoring">Scoring</a></li>
        <li><a href="#eligibility">Eligibility</a></li>
        <li><a href="#winning">Winning</a></li>
      </ul>
      <hr />

      <h2><a name="general">General</a></h2>
      <collapseCard>
        <div slot="header">Something about the site is broken!</div>
        <div slot="body">
          Sounds likely. I don't have a dedicated testing team and there are bound to be issues.
          Honestly, most issues can be fixed by refreshing the page.
          Beyond that, the best option is to open an issue on <a href="https://github.com/SteveF92/FantasyCritic" target="_blank">GitHub <font-awesome-icon icon="external-link-alt" size="sm" /></a>.
          Contacting me on Twitter or Discord is also an option.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">Something goes wrong when I try to confirm my email address.</div>
        <div slot="body">
          I've heard this from a few people now, and I haven't been able to identify the cause. It seems to be inconsistent as most people have no problems. I do have a list of potential solutions, though.
          <ol>
            <li>
              First, make sure that your email isn't already confirmed. Sometimes it works without showing the right message. Check your account page by clicking on your user name in the top
              right corner of the page.
            </li>
            <li>Try pressing CTRL+F5 when you are on the site and then clicking the link in the email again.</li>
            <li>Try an incognito window.</li>
            <li>Login first then try clicking the link.</li>
            <li>Logout first then try clicking the link.</li>
            <li>Try resending the email by clicking on "Manage Account" in the dropdown that comes up when you click your username in the top right, and using that link.</li>
            <li>Try a combination of these things.</li>
            <li>If all else fails, contact me.</li>
          </ol>
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">How can I follow updates for the site?</div>
        <div slot="body">
          Follow us on twitter <a href="https://twitter.com/fantasy_critic" target="_blank">@fantasy_critic <font-awesome-icon icon="external-link-alt" size="sm" /></a>
          and join our <a href="https://www.reddit.com/r/fantasycritic/" target="_blank">subreddit <font-awesome-icon icon="external-link-alt" size="sm" /></a>.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">Why should I play FantasyCritic?</div>
        <div slot="body">
          It’s a great catalyst for discussing video games with your friends, and gets everyone invested in checking out the year’s best video games.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">Does this cost money?</div>
        <div slot="body">
          Fantasy Critic is free to use. However, if you are able to, we would love it if you would support us on Patreon. Doing so will get you access to special
          rewards, including
          <router-link :to="{ name: 'fantasyCriticPlus' }" title="Fantasy Critic Plus">
            Fantasy Critic Plus.
          </router-link>
        </div>
      </collapseCard>


      <h2><a name="league-creation">League Creation</a></h2>
      <collapseCard>
        <div slot="header">How many players can be in a league?</div>
        <div slot="body">
          Anywhere from 2 to 20. Somewhere between 4 and 10 is probably optimal, and I'd be apprehensive about going over 14, but you can do it.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">How long does a league last?</div>
        <div slot="body">
          Each calendar year is one “game” of Fantasy Critic. Everyone starts fresh with a new roster at the start of each new year, but you are free to keep playing
          in the same league with the same people year after year.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">What does the league manager do?</div>
        <div slot="body">
          Similar to fantasy football, the league manager is in charge of inviting players, deciding on the settings for the league, and making administrative decisions
          throughout the year.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">What settings can be chosen by the league manager?</div>
        <div slot="body">
          <ul>
            <li>Amount of games per team.</li>
            <li>How many games to draft vs how many games to leave as mid-year pickups.</li>
            <li>Number of counter picks to draft per team.</li>
            <li>Whether or not to allow dropping games during the year.</li>
            <li>What games are eligible to be chosen (more on this below).</li>
            <li>Plus a few other settings.</li>
          </ul>
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">How many games should I allow per team?</div>
        <div slot="body">
          The league creation page will guide you through this. The answer will vary depending on how many people will be in your league and your players' familiarity with
          the site and games as a whole.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">The site says I need to create a "Publisher", what does that mean?</div>
        <div slot="body">
          So the "backstory" of Fantasy Critic is that you are the CEO of a video game publisher, and you need to decide which games currently in development are worth publishing. Your "Publisher" is
          just your "team" name. It's just a flavor thing. You can make it something serious or a big joke. Doesn't matter at all.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">What is a 'public league'?</div>
        <div slot="body">
          If you set your league to public, you allow people who are not in the league to view it. It's also added to a list of all public leagues for ease of finding popular public leagues.
          However, you still need an invite to join a public league. This setting only affects who can view a league, not who can join it.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">What is a 'test league'?</div>
        <div slot="body">
          Test leagues function exactly the same as normal leagues, except they don't affect the overall stats for games in the system, such as hype factor.
          This is to prevent people from manipulating the stats.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">My league was set to a test league and it won't let me change it back!</div>
        <div slot="body">
          League Managers can set a league as test league whenever they want, but cannot change a test league into a regular league. You will need to contact me if you want me to change your league.
          If I suspect that a league doesn't represent an honest effort by at least two seperate people, I reserve the right to set it as a test league.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">How do I delete a league?</div>
        <div slot="body">
          As of now, you'll need to contact me. Renaming your league to something like "delete me!" and sending me your league ID (it's what appears afer 'league/' and before '/{year}' in the URL) helps.
        </div>
      </collapseCard>
      <br />


      <h2><a name="drafting">Drafting</a></h2>
      <collapseCard>
        <div slot="header">How does the draft work?</div>
        <div slot="body">
          Your league will decide on the order that the teams will draft in. Then the league manager will decide when to begin the draft. Once it has started, the teams will
          choose games in the order that was chosen, snake style, until all teams have filled all of their draft game slots.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">What's a snake draft?</div>
        <div slot="body">
          A snake draft means that the person who goes last in a round of drafting, goes first in the next round. So with 4 teams the first 9 games
          would go Team A, B, C, D, D, C, B, A, A.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">How do I can change the draft order?</div>
        <div slot="body">
          Once all players have created their publishers, the option to "Edit Draft Order" will appear for the league manager. You can manually set an order or click the "randomize" button.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">During the draft, is there a time limit to picking games for each player?</div>
        <div slot="body">
          No, but leagues are encouraged to set a time to do their draft and have all players committed to completing it in one sitting. Even better, have all players in
          the same room or on a group video chat, to facilitate discussion of the games being selected.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">Am I picking games from a list or can I pick anything?</div>
        <div slot="body">
          Both, actually. I try to maintain a fairly inclusive list of upcoming games, which I call my "master game" list. If your game is in this list, it will show
          up when you search for it when drafting. If it doesn't, you can draft a "unlisted" game. I will frequently update the list of "master games", and the league manager
          can link a "unlisted" game to a "master game" at any point after the draft is completed, at no penalty to the player. However, if a game never recieves a score from
          OpenCritic, you will never get any points from it. They are fairly inclusive, but pick an extremely obscure game at your own risk.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">How do the rosters work?</div>
        <div slot="body">
          Each roster is made up of a number of “draft games”, “pickup games”, and “counter picks”. Draft games and pickup games are the games that you get points for.
          Any one game can only be on one roster within a single league. Counter picks work a bit different and are described below.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">What's this "Hype Factor" thing I see when I click on a game?</div>
        <div slot="body">
          "Hype Factor" is a measure of how highly anticipated a game is. It's a 0-100 value, with 100 being the best, derived from the percentage of leagues that draft a game, and what its average draft position is in those leagues.
          Only leagues that could have drafted a game, based on it's settings and the date when the league draft are counted in the percentage.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">How does the Auto Draft feature work?</div>
        <div slot="body">
          If a player is not available at the scheduled draft time, they can set themselves on "Auto Draft". When this setting is on, the site will choose their games for them. Players can use the
          "Watch List" feature to rank the games they want to draft, and if their list is empty or if every game on it is taken, the site will choose the game with the highest hype factor that is still available.
          Counterpicks are chosen in order of highest counterpick % accross the entire site. Both of these stats are viewable on the Master Games List.
        </div>
      </collapseCard>
      <br />


      <h2><a name="bidding-system">Bidding System</a></h2>
      <collapseCard>
        <div slot="header">What is a “draft game” vs a “pickup game”?</div>
        <div slot="body">
          Draft games are chosen during the draft - turn by turn at the start of the year. Pickup games are chosen over the course of year (after the draft) by bidding on them.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">How does the 'secret' bidding system work?</div>
        <div slot="body">
          If you’re familiar with Free Agency Acquisition Budget (FAAB) rules in fantasy football, it works like that. Essentially, any player that is interested in a game will place a secret "bid" for the game,
          using some of their available budget. Every player starts the year with $100 of budget. Then, on Saturday evenings at 8PM Eastern, the system will process those bids. The top bidder for any given game
          will recieve the game and spend the money that they bid. The other players keep their money but do not get the game.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">How does the 'public' bidding system work?</div>
        <div slot="body">
          This option is new for 2022. It is very similar to the standard 'secret' system, with one critical difference.
          The week is divided into two sections - the 'secret' section, and the 'public' section. From Saturday night, after the previous week of bids process, until Thursday night at 8PM Eastern,
          all bids are made privately, just like they are in the standard system.
          <br />
          <br />
          Then, on Thursday at 8PM, the names of all of the games that have active bids are made public to the rest of your league.
          The names of the players and the amounts being bid are not revealed, only the names of the games. From this point until bids process at 8PM on Saturday,
          players can edit the dollar amounts on the bids they have already made, or bid on a game that someone else has bid on that they haven't.
          However, players are <strong>not</strong> allowed to bid for a game that no one is currently bidding on, and players <strong>cannot</strong> cancel a bid entirely
          in the 'public bidding window'.
          <br />
          <br />
          The intention of this system to help prevent one "highly tuned in" player from dominating a league by always being the first to know about new games. If half of your players frequently
          find themselves saying something like: "Hey I didn't even know that game got announced and you already picked it up for $1. I have no shot at winning!", then you should consider this setting.
          If your leagues are already very competitive, we reccommend sticking with the 'secret' bidding system.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">What games are eligible?</div>
        <div slot="body">
          Same rules as during the draft. The system will warn you if you try to bid on a game that has already been released or already has reviews, but be aware that the game also must be eligible at the times the bid process,
          not just when you make the bid. For example, if you bid on a game on Tuesday, and reviews come out on Friday, then the bid will fail come Saturday.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">Will I still lose my money even if no one else is bidding on a game?</div>
        <div slot="body">
          Yes.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">Can I bid zero dollars on a game?</div>
        <div slot="body">
          Yes, unless your league sets a minimum bid amount.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">Can I bid on a game that doesn't come up when I search for games?</div>
        <div slot="body">
          Unfortunately no. To bid on a game, it must be in the master database. I try to stay on top of upcoming releases, but if a game that you want is missing,
          you can use the <router-link :to="{ name: 'masterGameRequest' }">master game request form</router-link>.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">When are bids processed?</div>
        <div slot="body">
          Bids (and drops, described below) are processed on Saturday evenings at 8PM Eastern (New York City time). While bids are processing, you will not be able to:
          <ul>
            <li>Make bids</li>
            <li>Cancel bids</li>
            <li>Request drops</li>
            <li>Cancel drops</li>
            <li>Add/Remove Publisher Games post draft (as a manager)</li>
            <li>Associate Publisher Games</li>
          </ul>
          The processing doesn't take long (no longer than 20 minutes, and probably much less). I only need to block these actions because if they happen during bid processing, it would cause conflicts with my algorithms.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">How are ties handled on bids?</div>
        <div slot="body">
          Here's the tiebreaker order:
          <ol>
            <li>Highest bid wins.</li>
            <li>Lowest projected points wins.</li>
            <li>The bid placed earlier wins.</li>
            <li>The player who was later in the draft order wins.</li>
          </ol>
          Twos special notes on 'Lowest Projected Points Wins':
          <ul>
            <li>The projections used are the 'advanced' projections.</li>
            <li>
              If your league uses 'special game slots', then points are counted towards your projection even if the game is not eligible for the slot it is in.
              The reason for this is to prevent people from intentionally moving games into slots they aren't eligible for in order to decrease their projected points
              while trying to win a bid tiebreak.
            </li>
          </ul>
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">Drops and bids are processed at the same time? How does that work?</div>
        <div slot="body">
          Drops run first, then bids.
          The system WILL allow you to bid on a game even if you have no slots left, but it will fail if there are no available when the bids actually run. This allows you to place a drop and a bid, have the drop
          process first, then you will have a spot for the bid to succeed.
          You can also use the 'conditional drop' option on your bid, which will drop the specified game <em>only if</em> you win the bid in question.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">How do "Pickup Counter Picks" work?</div>
        <div slot="body">
          Pickup counter picks are like normal counter picks, but instead of taking them in the draft, you take them at any point in the year using the bidding system. A few notes on these:
          <ul>
            <li>If you do not fill a pickup counter pick slot by the end of the year, you will lose 15 points.</li>
            <li>
              You cannot counter pick a game that is already set to "Will Not Release". You are free to counter pick a game that you <em>think</em> will get delayed, but counter picking a game that is already
              delayed isn't an interesting 'bet', so it's not allowed. If we did allow it, that would be the entire counter pick strategy. Wait for a game to get delayed, and then counter pick that.
            </li>
            <li>
              Sometimes, a game will have extremely credible reports of a delay, but the developer/publisher will not confirm the delay. In that case, we will markt the game as "Delay in Contention", and then the game will not be counter pickable.
            </li>
          </ul>
        </div>
      </collapseCard>
      <br />

      <h2><a name="dropping-games">Dropping Games</a></h2>
      <collapseCard>
        <div slot="header">How does the "dropping games" system work?</div>
        <div slot="body">
          Leagues can choose to make players able to "drop" a game that they either drafted or picked up through bidding, freeing the spot for them to pickup a new game. There's a number of ways to customize this feature.
          Three different options can be set to a number OR unlimited:
          <ul>
            <li><strong>Will Not Release Droppable Games: </strong>The number (or unlimited) of games that are confirmed to not release in the league year that can be dropped.</li>
            <li><strong>Will Release Droppable Games: </strong>The number (or unlimited) of games that ARE planned to release in the league year that can be dropped.</li>
            <li><strong>Free Droppable Games: </strong>The number (or unlimited) of games that can be dropped, whether they are planned to release or not.</li>
          </ul>

          Games that have already released or already have review scores are never droppable.
          <br />
          I admit that this feature is a little confusing and I'm still working on the best way to explain it. If you have questions, contact me and I'll try to help.
          Or, if you understand what I'm trying to say and have a better way to explain it, contact me and maybe your explanation will end up in this section.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">How do I drop a game?</div>
        <div slot="body">
          Games are dropped in similar way to the way games are bid on. Drops are requested, and then those drops are processed on Saturday night at 8PM Eastern Time. The reason for this is to make sure that someone can't drop a game
          that releases and gets a bad score before my database recognizes that the game has been reviewed. Doing it this way allows us to confirm that all games being dropped have up to date information.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">What happens if I drop a game that someone counter picked?</div>
        <div slot="body">
          Your game is dropped, but the counterpick the other player has stays and is not affected. However, if your league has the "counter picks block drops" option on, then you will not be able to drop a game if it has been counter picked by another player.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">How do I drop a game that is not linked to a master game?</div>
        <div slot="body">
          The dropping system won't let you. This is because it's impossible for me to decide how to handle a game that is not linked to a master game. Each league has to decide how to handle these cases themselves.
          League managers can use "Remove Publisher Game" to remove the game and "Edit Publishers" to manually adjust the amount of games the publisher has dropped.
        </div>
      </collapseCard>
      <br />

      <h2><a name="scoring">Scoring</a></h2>
      <collapseCard>
        <div slot="header">How does scoring work?</div>
        <div slot="body">
          A game's score is based on it’s score from <a href="https://opencritic.com/" target="_blank">opencritic.com <font-awesome-icon icon="external-link-alt" size="sm" /></a>. Your team will gain or lose points based upon how well your game reviews.
          Only critic reviews are taken into account. For every point above a 70 on OpenCritic, your game gets you one point. For every point below 70,
          you lose one point. If a game doesn’t release in the year you are playing, it is treated as getting a 70, and therefore 0 points.
          <br />
          <br />
          However, there are special rules to make certain points worth more or less. If a game scores above a 90, every point above 90 is worth two points. So if a game gets a 94,
          you get 20 points for 70 through 90, and 8 points for 90 through 94, for a total of 28 points for the 94.
          <br />
          <br />
          Another special rule is that there are diminishing returns on how much a very bad game hurts you. Each point from 60 through 70 loses you one point, but every 10 point range below that,
          each point is worth half of the range above it. I realize that sounds complicated, but I will provide examples below. The effect is that you can never lose more than 20 points, no matter how bad a game is.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">What is a “counter pick”?</div>
        <div slot="body">
          After everyone has drafted their roster of games, the players will draft their counter picks. A counter pick is a game from someone else’s roster, but instead of
          getting points for it, you LOSE points for it. The goal is to choose a game from someone else’s team that you think will be bad, or one that you think won't even release.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">If I counter pick a game that scores below a 70, will I gain points?</div>
        <div slot="body">
          Yes.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">If I counter pick a game that doesn't come out, what do I get?</div>
        <div slot="body">
          Both players get 0 points in this case. This should be considered a good result for the counter picker and a bad result for the person who actually chose the game.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">Can I see some scoring examples?</div>
        <div slot="body">
          <table class="table table-striped table-bordered table-responsive-sm">
            <thead>
              <tr class="bg-primary">
                <th>Game Name</th>
                <th>OpenCritic Score</th>
                <th>Fantasy Points</th>
                <th>Counter Pick Points</th>
                <th>Notes</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td>Dragon Ball FighterZ</td>
                <td>86</td>
                <td>16</td>
                <td>-16</td>
                <td></td>
              </tr>
              <tr>
                <td>Shadow of the Tomb Raider</td>
                <td>80</td>
                <td>10</td>
                <td>-10</td>
                <td></td>
              </tr>
              <tr>
                <td>Red Dead Redemption 2</td>
                <td>97</td>
                <td>34</td>
                <td>-34</td>
                <td>
                  1 point per point for 70->90
                  <br />
                  2 points per point for 90->97
                </td>
              </tr>
              <tr>
                <td>Fallout 76</td>
                <td>52</td>
                <td>-14</td>
                <td>14</td>
                <td>
                  -1 point per point for 60->70
                  <br />
                  -0.5 points per point for 52->60
                </td>
              </tr>
              <tr>
                <td>Left Alive</td>
                <td>38</td>
                <td>-17.75</td>
                <td>17.75</td>
                <td>
                  -1 point per point for 60->70
                  <br />
                  -0.5 points per point for 50->60
                  <br />
                  -0.25 per point for 40->50
                  <br />
                  -0.125 per point for 38->40
                </td>
              </tr>
              <tr>
                <td>Halo Infinite</td>
                <td>Delayed to 2021</td>
                <td>0</td>
                <td>0</td>
                <td>This assumes a 2020 league.</td>
              </tr>
            </tbody>
          </table>
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">What order are counter picks chosen in?</div>
        <div slot="body">
          Counter picks are chosen in a snake draft - just like the regular games. However, the player who went LAST in the regular draft always goes first when it comes time to draft counter picks.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">What's a manual score?</div>
        <div slot="body">
          If for some reason you don't agree with my methodologies, you can choose to manually assign a score. For example, if a game is released but never appears on OpenCritic, you could choose to add
          manual score from another site - hopefully one you decided on ahead of time. Or, if you think you should be able to draft specific episodes of an episodic series, you'll have to manually score
          those as well. See the <a href="#eligibility">eligibility</a> section for more on episodic games.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">My game has a critic score but no points. What gives?</div>
        <div slot="body">
          The game needs to release before you get the points. So if reviews come out 3 days before the game, you will see a critic score but no points for a few days. The only time this will actually
          make a difference is if a game comes out in early January but has reviews come out in late December. In that case, you won't get the points for the year, because the game didn't come out.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">I see a score on OpenCritic but not in my league. Why?</div>
        <div slot="body">
          My database probably hasn't refreshed yet. That happens every two hours. I don't want to hit OpenCritic's API more frequently than I have to, as doing so
          involves a bunch of rapid requests. Additionally, I have to manually enter the "OpenCritic ID" for every game in the master games list.
          OpenCritic doesn't usually add a game until it is close to release, so I need to check frequently to see if they have added new games. If you notice a particular
          game that is missing a score, feel free to let me know. If a score is just out of sync, it'll be fixed soon.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">I see a score on the site but not on OpenCritic? Why?</div>
        <div slot="body">
          This typically happens when a game is very new. When you use the OpenCritic API, there are actually two seperate scores: "Average Score", and "Top Critic Score". The "Average Score" is simply the average of every scored review that the site lists.
          The "Top Critic Score" is the average of all scored reviews from critic who are marked as "Top Critics". In both cases, there must be at least 3 reviews in the average before the API will return a number. The score that is displayed on OpenCritic's UI is the
          "Top Critic Score", and that is what Fantasy Critic uses - when available. If there is no "Top Critic Score" available, but there is an "Average Score", then Fantasy Critic will use that. This will happen if, for example, OpenCritic has three scored reviews listed, but
          only 2 of them are from "Top Critics". In these cases, the score is likely to change significantly as very few reviews are accounted for so far.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">When are critic scores locked in?</div>
        <div slot="body">
          At the very end of the year. Usually scores don't change after the first few weeks a game has been out, but there's no reason to "lock" scores until it's time to determine the winner.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">How do projected points work?</div>
        <div slot="body">
          The "simple" projected points are just the average number of points a player receives for a game. So, the average across the whole site. The "Advanced Projections" go much further, and attempt to
          project what score an individual game will get using a linear regression algorithm based on popularity of games on the site versus the score the game ultimately gets.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">What's the "Pre-Release projected points"?</div>
        <div slot="body">
          This is the last "projected points" values before a game was reviewed. The idea is to compare what a game was projected to get vs. what it actually got.
          I'm still testing this feature, and any games that were released before 2019-10-25 don't show up accurately for this field due to the way I was handling projections before that date.
        </div>
      </collapseCard>
      <br />

      <h2><a name="eligibility">Eligibility</a></h2>
      <collapseCard>
        <div slot="header">How does the new "tags" system work?</div>
        <div slot="body">
          New for 2021, chosing what games are eligible has been streamlined. All games now have one or more "tags", such as "Remake", "New Game", or "Yearly Installment". The league manager can choose
          what tags are and are not allowed. Mouse over any tag to see a description of it.
          <br />
          <br />
          <div v-if="allTags">
            <span v-for="tag in allTags">
              <masterGameTagBadge :tagName="tag.name"></masterGameTagBadge>
            </span>
          </div>
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">
          I don't agree with your tags! My league allows "remakes" but "Tony Hawk 1 + 2" is really just a remaster. It shouldn't be allowed!
        </div>
        <div slot="body">
          It's up to your league and ultimately up to your league manager. League managers can override the eligibility rules and assign
          the game you want to your team during the draft. Additionally, outside of the draft, the league manager can manually 'allow' or 'ban'
          a specific game, which either allows or blocks players from bidding on the game. Whether you are making these exemptions at the draft or
          during the year, your league should agree on whether or not they want to allow the exception.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">
          How do episodic games work?
        </div>
        <div slot="body">
          If no game from an episodic series has come out, you can select the game as a whole. If episodes have already come out, then the game will not be eligible.
          If you do have an episodic game on your roster, and only some of the episodes come out, then we use the average critic score of the released episodes.
          If the entire game has been released, the game usually receives reviews for the game as whole, and we use that score.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">
          How do early access games work?
        </div>
        <div slot="body">
          There are two tags related to early access games. You can use them to chooses to ban games from being drafted/picked up if they are already playable in early access, or,
          alternatively, if they are even PLANNED for early access.
          It's important to note that games will not recieve points until they are "fully released", meaning games do not get points while still in early access. This is true even if they get reviews while in early access, which is rare.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">
          Why can't I ban 'Planned for Early Access' without also banning 'Currently in Early Access'?
        </div>
        <div slot="body">
          This used to be allowed, but truthfully never should have been. Consider this scenerio:
          <ul>
            <li>A game is announced on Feburary 5th, 2022. It will go into early access on March 3rd, 2022, and have a full release on October 10th, 2022.</li>
            <li>Let's say you want to allow both 'Planned for Early Access' and 'Currently in Early Access'.</li>
            <li class="nested-list">
              <ul>
                <li>
                  On Feburary 5th, 2022, the game becomes draftable/biddable. It remains draftable/biddable up until either it's full release on October 10th, 2022,
                  or when it gets reviews, whichever comes first.
                </li>
                <li>
                  Most of the time, a game will not get critic reviews while in early access. Howeever, it does happen sometimes, and that does mean you can't draft or bid on the game.
                </li>
              </ul>
            </li>
            <li>Now consider what happens if you BAN both 'Planned for Early Access' and 'Currently in Early Access'.</li>
            <li class="nested-list">
              <ul>
                <li>
                  On Feburary 5th, 2022, the game is immediately 'Planned for Early Access', so you cannot ever bid on it or draft it.
                </li>
              </ul>
            </li>
            <li>Now consider what happens if you allow 'Planned for Early Access', but not 'Currently in Early Access'.</li>
            <li class="nested-list">
              <ul>
                <li>
                  On Feburary 5th, 2022, the game becomes draftable/biddable. It remains draftable/biddable up until either it enters early access on March 3rd, 2022.
                </li>
              </ul>
            </li>
            <li>Finally, consider what happens if you allow 'Currently in Early Access', but not 'Planned for Early Access'.</li>
            <li class="nested-list">
              <ul>
                <li>
                  On Feburary 5th, 2022, the game is immediately 'Planned for Early Access', so you cannot bid on it or draft it. However, when the game enters early access on March 3rd, 2022,
                  now it is 'Currently in Early access', which IS allowed, so now you CAN bid on it/draft it up until it's full release on October 10th, 2022,
                  or when it gets reviews, whichever comes first.
                </li>
                <li>
                  We believe that this setup should not be allowed, due to it's confusing nature. It's also a nightmare to code for.
                </li>
              </ul>
            </li>
          </ul>
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">
          Why can't I ban 'Will Release Internationally First' without also banning 'Released Internationally'?
        </div>
        <div slot="body">
          See the above answer about 'Planned for Early Access' and 'Currently in Early Access', because the same logic applies.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">
          How are "re-releases" of games that were never released in North America handled?
        </div>
        <div slot="body">
          This one is complicated. For example, if a game came out in Japan, but not North America, and then some time later, the game is updated with a new release in Japan and <em>that</em> release comes out in North America,
          the tags on the "updated release" will vary based upon how much the updated release differs from the original release. As a general rule, a game cannot be a "remake" or a "remaster" of a game
          that never released in North America, so all of these will be "New Game" or "New Gaming Franchise".
          However, if a game only has minor updates over the original, then it will get the "Released Internationally" tag.
          One way to think about this is, what would be the tags on the game if Fantasy Critic was a Japan-based site?

          <br />
          <br />
          Here are some examples for clarity:
          <br />
          <br />
          <table class="table table-striped table-bordered table-responsive-sm">
            <thead>
              <tr class="bg-primary">
                <th>Game Name</th>
                <th>Tags in Japanese Context</th>
                <th>Tags in North American Context</th>
                <th>Notes</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td>Earthbound Beginnings</td>
                <td>
                  <masterGameTagBadge tagName="Port"></masterGameTagBadge>
                </td>
                <td>
                  <masterGameTagBadge tagName="NewGame"></masterGameTagBadge>
                  <masterGameTagBadge tagName="ReleasedInternationally"></masterGameTagBadge>
                </td>
                <td>
                  This is a port of Mother 1. This version is not "enough of remake" to consider it a game that "has not been released internationally".
                </td>
              </tr>
              <tr>
                <td>River City Girls Zero</td>
                <td>
                  <masterGameTagBadge tagName="Remaster"></masterGameTagBadge>
                </td>
                <td>
                  <masterGameTagBadge tagName="NewGame"></masterGameTagBadge>
                  <masterGameTagBadge tagName="ReleasedInternationally"></masterGameTagBadge>
                </td>
                <td>
                  This game was originally released on the Super Famicom in 1994. This version is not "enough of remake" to consider it a game that "has not been released internationally".
                </td>
              </tr>
              <tr>
                <td>The Great Ace Attorney Chronicles</td>
                <td>
                  <masterGameTagBadge tagName="Remaster"></masterGameTagBadge>
                </td>
                <td>
                  <masterGameTagBadge tagName="NewGame"></masterGameTagBadge>
                  <masterGameTagBadge tagName="ReleasedInternationally"></masterGameTagBadge>
                </td>
                <td>
                  This game is a compilation of two games that never came out in North America. It is not "enough of remake" to consider it a game that "has not been released internationally".
                </td>
              </tr>
              <tr>
                <td>Bravely Default</td>
                <td>
                  <masterGameTagBadge tagName="DirectorsCut"></masterGameTagBadge>
                </td>
                <td>
                  <masterGameTagBadge tagName="NewGamingFranchise"></masterGameTagBadge>
                  <masterGameTagBadge tagName="ReleasedInternationally"></masterGameTagBadge>
                </td>
                <td>
                  Bravely Default was originally a Japan-only 3DS game called "Bravely Default: Flying Fairy", released in 2012. In 2013, an expanded version was released, again only in Japan, called "Bravely Default: For The Sequel". In a Japanese context,
                  we would consider this a "Director's Cut". Then, in 2014, "Bravely Default: For The Sequel" was released in North America as "Bravely Default". Both the original version and the expanded version had previously released outside North America, and
                  the expanded version is not "enough of a remake" to consider it a "different game" anyway, so this would be tagged as a "New Gaming Franchise" and "Released Internationally". It's worth noting that the tags would be the same even if the second version
                  released in Japan and North America at the same time.
                </td>
              </tr>
              <tr>
                <td>Famicom Detective Club</td>
                <td>
                  <masterGameTagBadge tagName="Remake"></masterGameTagBadge>
                </td>
                <td>
                  <masterGameTagBadge tagName="NewGamingFranchise"></masterGameTagBadge>
                </td>
                <td>
                  This game is "more of a remake" with respect to the original game, compared to the games above. It's not fair to call the original Famicom game as the "same game", meaning that <em>this</em> game has
                  never released anywhere, and therefore it does not get a "Released Internationally" tag. In addition, there are no games in this franchise previously released in North America, so it gets the
                  "New Gaming Franchise" tag.
                </td>
              </tr>
              <tr>
                <td>Live A Live</td>
                <td>
                  <masterGameTagBadge tagName="Remake"></masterGameTagBadge>
                </td>
                <td>
                  <masterGameTagBadge tagName="NewGamingFranchise"></masterGameTagBadge>
                </td>
                <td>
                  Like Famicom Dective Club, this game is "more of a remake" with respect to the original game, compared to the games above. It's not fair to call the original Super Famicom game as the "same game", meaning that <em>this</em> game has
                  never released anywhere, and therefore it does not get a "Released Internationally" tag. In addition, there are no games in this franchise previously released in North America, so it gets the
                  "New Gaming Franchise" tag.
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">
          What's the definition of a 'New Gaming Franchise'?
        </div>
        <div slot="body">
          I admit this is tricky, so here's a list of criteria that we consider:
          <br />
          <br />
          <h6>Signs a game <em>is</em> a 'New Gaming Franchise':</h6>
          <ul>
            <li>Is this game an obvious, clear as day new IP?</li>
            <li class="nested-list">
              <ul>
                <li>Celeste, The Last of Us, Death's Door</li>
              </ul>
            </li>
            <li>Is this game a 'spiritual successor' but not the same universe?</li>
            <li class="nested-list">
              <ul>
                <li>Elden Ring, Back 4 Blood</li>
              </ul>
            </li>
            <li>Is this game based on a property that has never been a video game before?</li>
            <li class="nested-list">
              <ul>
                <li>Cyberpunk 2077, Metro 2033</li>
              </ul>
            </li>
            <li>Is this game based on a public domain IP, and <em>this adaptation</em> is new?</li>
            <li class="nested-list">
              <ul>
                <li>Sherlock Holmes: The Mystery of the Mummy (2002), The Sinking City</li>
              </ul>
            </li>
            <li>Is this game 'very on brand' for a developer but nonetheless not a sequel?</li>
            <li class="nested-list">
              <ul>
                <li>Triangle Strategy</li>
              </ul>
            </li>
          </ul>

          <h6>Signs a game <em>is not</em> a 'New Gaming Franchise'</h6>
          <ul>
            <li>Is this game better defined as a sequel, spinoff, or reboot?</li>
            <li class="nested-list">
              <ul>
                <li>Tomb Raider (2013), Prince of Persia (2008), Gears Tactics, Project L (League of Legends fighting game)</li>
              </ul>
            </li>
            <li>Is this game based on an IP, that is <em>not</em> in the public domain, that IS represented in video games?</li>
            <li class="nested-list">
              <ul>
                <li>Spider Man (2018), Gotham Knights, Wonder Woman</li>
              </ul>
            </li>
          </ul>
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">What is a 'Special Game Slot'?</div>
        <div slot="body">
          New for 2022, special roster slots allow your league to specify a requirement of certain game types in each roster,
          including games that are banned in your normal league rules. For example even if your league doesn’t allow yearly installments,
          you can require each publisher to have one yearly installment on their roster. This slot will only give points for games that meet its designated requirements.
          <br />
          <br />
          A good way to think of this is like "positions" in Fantasy Football. You don't just draft 11 "players", you have quarterbacks, running backs, wide recievers, etc, each of which
          have designated slots. In Fantasy Critic, these "positions" are the tags, so you can have "Remake" slot, a "Currently in Early Access" slot, and a "Virtual Reality" slot, if you so desire.
        </div>
      </collapseCard>
      <collapseCard>
        <div slot="header">How do I move games into a special slot?</div>
        <div slot="body">
          They system should assign a game to the best slot when drafted or picked up.
          However, if you do need to move a game in your roster, this can be done at the My Publisher Details page, found under the Player Actions tab.
        </div>
      </collapseCard>
      <br />

      <h2><a name="winning">Winning</a></h2>
      <collapseCard>
        <div slot="header">What do I get if I win?</div>
        <div slot="body">
          That’s totally up to your league. This site doesn’t provide any prizes. Look to fantasy sports for advice on how to structure any winnings you want to play for. My personal favorite
          is to end the year with a group dinner where the winner doesn't pay, and then do the draft for the next year right after that.
        </div>
      </collapseCard>
    </div>
  </div>
</template>
<script>
import CollapseCard from '@/components/modules/collapseCard';
import MasterGameTagBadge from '@/components/modules/masterGameTagBadge';
const TIMEOUT = 1;

export default {
  components: {
    CollapseCard,
    MasterGameTagBadge
  },
  data() {
    return {
      lastID: 1
    };
  },
  computed: {
    allTags() {
      return this.$store.getters.allTags;
    }
  },
  methods: {
    getCollapseID() {
      let thisID = this.lastID;
      this.lastID = this.lastID + 1;
      return thisID;
    },
    scrollTo(hashtag){
      setTimeout(() => { location.href = hashtag }, TIMEOUT)
    }
  },
  mounted() {
    // From testing, without a brief timeout, it won't work.
    if (this.$route.hash) {
      setTimeout(() => this.scrollTo(this.$route.hash), TIMEOUT)
    }
  }
};
</script>
<style scoped>
  .fc-plus-logo {
    height: 24px;
    padding-bottom: 3px;
    margin-right: 3px;
  }
</style>
