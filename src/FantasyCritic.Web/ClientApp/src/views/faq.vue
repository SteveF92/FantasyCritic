<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <h1>Frequently Asked Questions</h1>
      <hr />

      <h2>Contents</h2>
      <ul>
        <li><router-link :to="{ hash: '#general' }">General</router-link></li>
        <li><router-link :to="{ hash: '#league-creation' }">League Creation</router-link></li>
        <li><router-link :to="{ hash: '#drafting' }">Drafting</router-link></li>
        <li><router-link :to="{ hash: '#bidding-system' }">Bidding System</router-link></li>
        <li><router-link :to="{ hash: '#dropping-games' }">Dropping Games</router-link></li>
        <li><router-link :to="{ hash: '#trades' }">Trades</router-link></li>
        <li><router-link :to="{ hash: '#scoring' }">Scoring</router-link></li>
        <li><router-link :to="{ hash: '#eligibility' }">Eligibility</router-link></li>
        <li><router-link :to="{ hash: '#winning' }">Winning</router-link></li>
      </ul>

      <hr />

      <h2><a id="general">General</a></h2>
      <collapseCard>
        <template #header>Something about the site is broken!</template>
        <template #body>
          Sounds likely. I don't have a dedicated testing team and there are bound to be issues. Honestly, most issues can be fixed by refreshing the page. Beyond that, the best option is to open an
          issue on
          <a href="https://github.com/SteveF92/FantasyCritic" target="_blank">
            GitHub
            <font-awesome-icon icon="external-link-alt" size="sm" />
          </a>
          . Contacting me on Twitter or Discord is also an option.
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>Something goes wrong when I try to confirm my email address.</template>
        <template #body>
          I've heard this from a few people now, and I haven't been able to identify the cause. It seems to be inconsistent as most people have no problems. I do have a list of potential solutions,
          though.
          <ol>
            <li>
              First, make sure that your email isn't already confirmed. Sometimes it works without showing the right message. Check your account page by clicking on your user name in the top right
              corner of the page.
            </li>
            <li>Try pressing CTRL+F5 when you are on the site and then clicking the link in the email again.</li>
            <li>Try an incognito window.</li>
            <li>Login first then try clicking the link.</li>
            <li>Logout first then try clicking the link.</li>
            <li>Try resending the email by clicking on "Manage Account" in the dropdown that comes up when you click your username in the top right, and using that link.</li>
            <li>Try a combination of these things.</li>
            <li>If all else fails, contact me.</li>
          </ol>
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>How can I follow updates for the site?</template>
        <template #body>
          Follow us on twitter
          <a href="https://twitter.com/fantasy_critic" target="_blank">
            @fantasy_critic
            <font-awesome-icon icon="external-link-alt" size="sm" />
          </a>
          and join our
          <a href="https://www.reddit.com/r/fantasycritic/" target="_blank">
            subreddit
            <font-awesome-icon icon="external-link-alt" size="sm" />
          </a>
          .
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>Why should I play FantasyCritic?</template>
        <template #body>It’s a great catalyst for discussing video games with your friends, and gets everyone invested in checking out the year’s best video games.</template>
      </collapseCard>
      <collapseCard>
        <template #header>Does this cost money?</template>
        <template #body>
          Fantasy Critic is free to use. However, if you are able to, we would love it if you would support us on Patreon. Doing so will get you access to special rewards, including
          <router-link :to="{ name: 'fantasyCriticPlus' }" title="Fantasy Critic Plus">Fantasy Critic Plus.</router-link>
        </template>
      </collapseCard>

      <h2><a id="league-creation">League Creation</a></h2>
      <collapseCard>
        <template #header>How many players can be in a league?</template>
        <template #body>Anywhere from 2 to 20. Somewhere between 4 and 10 is probably optimal, and I'd be apprehensive about going over 14, but you can do it.</template>
      </collapseCard>
      <collapseCard>
        <template #header>How long does a league last?</template>
        <template #body>
          Each calendar year is one “game” of Fantasy Critic. Everyone starts fresh with a new roster at the start of each new year, but you are free to keep playing in the same league with the same
          people year after year.
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>What does the league manager do?</template>
        <template #body>
          Similar to fantasy football, the league manager is in charge of inviting players, deciding on the settings for the league, and making administrative decisions throughout the year.
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>What settings can be chosen by the league manager?</template>
        <template #body>
          <ul>
            <li>Amount of games per team.</li>
            <li>How many games to draft vs how many games to leave as mid-year pickups.</li>
            <li>Number of counter picks to draft per team.</li>
            <li>Whether or not to allow dropping games during the year.</li>
            <li>What games are eligible to be chosen (more on this below).</li>
            <li>Plus a few other settings.</li>
          </ul>
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>How many games should I allow per team?</template>
        <template #body>
          The league creation page will guide you through this. The answer will vary depending on how many people will be in your league and your players' familiarity with the site and games as a
          whole.
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>The site says I need to create a "Publisher", what does that mean?</template>
        <template #body>
          So the "backstory" of Fantasy Critic is that you are the CEO of a video game publisher, and you need to decide which games currently in development are worth publishing. Your "Publisher" is
          just your "team" name. It's just a flavor thing. You can make it something serious or a big joke. Doesn't matter at all.
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>What is a 'public league'?</template>
        <template #body>
          If you set your league to public, you allow people who are not in the league to view it. It's also added to a list of all public leagues for ease of finding popular public leagues. However,
          you still need an invite to join a public league. This setting only affects who can view a league, not who can join it.
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>What is a 'test league'?</template>
        <template #body>
          Test leagues function exactly the same as normal leagues, except they don't affect the overall stats for games in the system, such as hype factor. This is to prevent people from manipulating
          the stats.
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>My league was set to a test league and it won't let me change it back!</template>
        <template #body>
          League Managers can set a league as test league whenever they want, but cannot change a test league into a regular league. You will need to contact me if you want me to change your league.
          If I suspect that a league doesn't represent an honest effort by at least two seperate people, I reserve the right to set it as a test league.
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>How do I delete a league?</template>
        <template #body>
          As of now, you'll need to contact us. Renaming your league to something like "delete me!" and sending me your league ID (it's what appears afer 'league/' and before '/{year}' in the URL)
          helps.
        </template>
      </collapseCard>
      <br />

      <h2><a id="drafting">Drafting</a></h2>
      <collapseCard>
        <template #header>How does the draft work?</template>
        <template #body>
          Your league will decide on the order that the teams will draft in. Then the league manager will decide when to begin the draft. Once it has started, the teams will choose games in the order
          that was chosen, snake style, until all teams have filled all of their draft game slots.
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>What's a snake draft?</template>
        <template #body>
          A snake draft means that the person who goes last in a round of drafting, goes first in the next round. So with 4 teams the first 9 games would go Team A, B, C, D, D, C, B, A, A.
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>How do I can change the draft order?</template>
        <template #body>
          Once all players have created their publishers, the option to "Edit Draft Order" will appear for the league manager. You can manually set an order or click the "randomize" button.
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>During the draft, is there a time limit to picking games for each player?</template>
        <template #body>
          No, but leagues are encouraged to set a time to do their draft and have all players committed to completing it in one sitting. Even better, have all players in the same room or on a group
          video chat, to facilitate discussion of the games being selected.
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>Am I picking games from a list or can I pick anything?</template>
        <template #body>
          Both, actually. I try to maintain a fairly inclusive list of upcoming games, which I call my "master game" list. If your game is in this list, it will show up when you search for it when
          drafting. If it doesn't, you can draft a "unlisted" game. I will frequently update the list of "master games", and the league manager can link a "unlisted" game to a "master game" at any
          point after the draft is completed, at no penalty to the player. However, if a game never recieves a score from OpenCritic, you will never get any points from it. They are fairly inclusive,
          but pick an extremely obscure game at your own risk.
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>How do the rosters work?</template>
        <template #body>
          Each roster is made up of a number of “draft games”, “pickup games”, and “counter picks”. Draft games and pickup games are the games that you get points for. Any one game can only be on one
          roster within a single league. Counter picks work a bit different and are described below.
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>What's this "Hype Factor" thing I see when I click on a game?</template>
        <template #body>
          "Hype Factor" is a measure of how highly anticipated a game is. It's a 0-100 value, with 100 being the best, derived from the percentage of leagues that draft a game, and what its average
          draft position is in those leagues. Only leagues that could have drafted a game, based on it's settings and the date when the league draft are counted in the percentage.
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>How does the Auto Draft feature work?</template>
        <template #body>
          If a player is not available at the scheduled draft time, they can set themselves on "Auto Draft". When this setting is on, the site will choose their games for them. Players can use the
          "Watch List" feature to rank the games they want to draft, and if their list is empty or if every game on it is taken, the site will choose the game with the highest hype factor that is
          still available. Counterpicks are chosen in order of highest counterpick % accross the entire site. Both of these stats are viewable on the Master Games List.
        </template>
      </collapseCard>
      <br />

      <h2><a id="bidding-system">Bidding System</a></h2>
      <collapseCard>
        <template #header>What is a “draft game” vs a “pickup game”?</template>
        <template #body>
          Draft games are chosen during the draft - turn by turn at the start of the year. Pickup games are chosen over the course of year (after the draft) by bidding on them.
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>How does the 'secret' bidding system work?</template>
        <template #body>
          If you’re familiar with Free Agency Acquisition Budget (FAAB) rules in fantasy football, it works like that. Essentially, any player that is interested in a game will place a secret "bid"
          for the game, using some of their available budget. Every player starts the year with $100 of budget. Then, on Saturday evenings at 8PM Eastern, the system will process those bids. The top
          bidder for any given game will recieve the game and spend the money that they bid. The other players keep their money but do not get the game.
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>How does the 'public' bidding system work?</template>
        <template #body>
          <p>
            This option is new for 2022. It is very similar to the standard 'secret' system, with one critical difference. The week is divided into two sections - the 'secret' section, and the
            'public' section. From Saturday night, after the previous week of bids process, until Thursday night at 8PM Eastern, all bids are made privately, just like they are in the standard system.
          </p>

          <p>
            Then, on Thursday at 8PM, the names of all of the games that have active bids are made public to the rest of your league. The names of the players and the amounts being bid are not
            revealed, only the names of the games. From this point until bids process at 8PM on Saturday, players can edit the dollar amounts on the bids they have already made, or bid on a game that
            someone else has bid on that they haven't. However, players are
            <strong>not</strong>
            allowed to bid for a game that no one is currently bidding on, and players
            <strong>cannot</strong>
            cancel a bid entirely in the 'public bidding window'.
          </p>

          <p>
            The intention of this system to help prevent one "highly tuned in" player from dominating a league by always being the first to know about new games. If half of your players frequently
            find themselves saying something like: "Hey I didn't even know that game got announced and you already picked it up for $1. I have no shot at winning!", then you should consider this
            setting. If your leagues are already very competitive, we reccommend sticking with the 'secret' bidding system.
          </p>
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>What games are eligible?</template>
        <template #body>
          Same rules as during the draft. The system will warn you if you try to bid on a game that has already been released or already has reviews, but be aware that the game also must be eligible
          at the times the bid process, not just when you make the bid. For example, if you bid on a game on Tuesday, and reviews come out on Friday, then the bid will fail come Saturday.
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>Will I still lose my money even if no one else is bidding on a game?</template>
        <template #body>Yes.</template>
      </collapseCard>
      <collapseCard>
        <template #header>Can I bid zero dollars on a game?</template>
        <template #body>Yes, unless your league sets a minimum bid amount.</template>
      </collapseCard>
      <collapseCard>
        <template #header>Can I bid on a game that doesn't come up when I search for games?</template>
        <template #body>
          Unfortunately no. To bid on a game, it must be in the master database. I try to stay on top of upcoming releases, but if a game that you want is missing, you can use the
          <router-link :to="{ name: 'masterGameRequest' }">master game request form</router-link>
          .
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>When are bids processed?</template>
        <template #body>
          Bids (and drops, described below) are processed on Saturday evenings at 8PM Eastern (New York City time). While bids are processing, you will not be able to:
          <ul>
            <li>Make bids</li>
            <li>Cancel bids</li>
            <li>Request drops</li>
            <li>Cancel drops</li>
            <li>Add/Remove Publisher Games post draft (as a manager)</li>
            <li>Associate Publisher Games</li>
          </ul>
          The processing doesn't take long (no longer than 20 minutes, and probably much less). I only need to block these actions because if they happen during bid processing, it would cause
          conflicts with my algorithms.
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>How are ties handled on bids?</template>
        <template #body>
          The default tiebreaker order is:
          <ol>
            <li>Highest bid wins.</li>
            <li>Lowest projected points wins.</li>
            <li>The bid placed earlier wins.</li>
            <li>The player who was later in the draft order wins.</li>
          </ol>
          <p>However, leagues can choose to flip the 2nd and 3rd options, so that after highest bids, the next tiebreaker is the bid placed earlier.</p>
          <p>
            If your league uses 'special game slots', then points are counted towards your projection even if the game is not eligible for the slot it is in. The reason for this is to prevent players
            from intentionally moving games into slots they aren't eligible for in order to decrease their projected points while trying to win a bid tiebreak.
          </p>
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>Drops and bids are processed at the same time? How does that work?</template>
        <template #body>
          Drops run first, then bids. The system WILL allow you to bid on a game even if you have no slots left, but it will fail if there are no available when the bids actually run. This allows you
          to place a drop and a bid, have the drop process first, then you will have a spot for the bid to succeed. You can also use the 'conditional drop' option on your bid, which will drop the
          specified game
          <em>only if</em>
          you win the bid in question.
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>How do "Pickup Counter Picks" work?</template>
        <template #body>
          Pickup counter picks are like normal counter picks, but instead of taking them in the draft, you take them at any point in the year using the bidding system. A few notes on these:
          <ul>
            <li>If you do not fill a pickup counter pick slot by the end of the year, you will lose 15 points.</li>
            <li>Your league can also set a "counter pick deadline", after which, you can only chose to counter pick a game with a confirmed release date.</li>
            <li class="nested-list">
              <ul>
                <li>A 'confirmed release date' means a single date, like "December 12th, 2022". A release date of "2022" does not count.</li>
                <li>The intention for this is to prevent players from waiting until the very end of the year to counter pick a game that is almost certain to not release that year.</li>
              </ul>
            </li>
            <li>
              You cannot counter pick a game that is already set to "Will Not Release". You are free to counter pick a game that you
              <em>think</em>
              will get delayed, but counter picking a game that is already delayed isn't an interesting 'bet', so it's not allowed. If we did allow it, that would be the entire counter pick strategy.
              Wait for a game to get delayed, and then counter pick that.
            </li>
            <li>
              Sometimes, a game will have extremely credible reports of a delay, but the developer/publisher will not confirm the delay. In that case, we will markt the game as "Delay in Contention",
              and then the game will not be counter pickable.
            </li>
          </ul>
        </template>
      </collapseCard>
      <br />

      <h2><a id="dropping-games">Dropping Games</a></h2>
      <collapseCard>
        <template #header>How does the "dropping games" system work?</template>
        <template #body>
          Leagues can choose to make players able to "drop" a game that they either drafted or picked up through bidding, freeing the spot for them to pickup a new game. There's a number of ways to
          customize this feature. Three different options can be set to a number OR unlimited:
          <ul>
            <li>
              <strong>Will Not Release Droppable Games:</strong>
              The number (or unlimited) of games that are confirmed to not release in the league year that can be dropped.
            </li>
            <li>
              <strong>Will Release Droppable Games:</strong>
              The number (or unlimited) of games that ARE planned to release in the league year that can be dropped.
            </li>
            <li>
              <strong>Free Droppable Games:</strong>
              The number (or unlimited) of games that can be dropped, whether they are planned to release or not.
            </li>
          </ul>

          <p>Games that have already released or already have review scores are never droppable.</p>

          <p>
            I admit that this feature is a little confusing and I'm still working on the best way to explain it. If you have questions, contact me and I'll try to help. Or, if you understand what I'm
            trying to say and have a better way to explain it, contact me and maybe your explanation will end up in this section.
          </p>
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>How do I drop a game?</template>
        <template #body>
          Games are dropped in similar way to the way games are bid on. Drops are requested, and then those drops are processed on Saturday night at 8PM Eastern Time. The reason for this is to make
          sure that someone can't drop a game that releases and gets a bad score before my database recognizes that the game has been reviewed. Doing it this way allows us to confirm that all games
          being dropped have up to date information.
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>What happens if I drop a game that someone counter picked?</template>
        <template #body>
          Your game is dropped, but the counterpick the other player has stays and is not affected. However, if your league has the "counter picks block drops" option on, then you will not be able to
          drop a game if it has been counter picked by another player.
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>How do I drop a game that is not linked to a master game?</template>
        <template #body>
          The dropping system won't let you. This is because it's impossible for me to decide how to handle a game that is not linked to a master game. Each league has to decide how to handle these
          cases themselves. League managers can use "Remove Publisher Game" to remove the game and "Edit Publishers" to manually adjust the amount of games the publisher has dropped.
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>How do "Super Drops" work?</template>
        <template #body>
          <p>
            A super drop allows a player to drop any one of their games, immediately, no questions asked. Even if the game is already released, even if it's been counter picked, even if it is a
            counter pick. You also don't need to wait for the drop to process on Saturday, it happens immediately instead.
          </p>
          <p>
            This mechanic is intended to help players that have fallen far behind in their league. League managers can decide to grant a player a super drop manually using the "Edit Publishers"
            option. Additionally, leagues can also turn on "Automatic Super Drops". If this option is enabled, starting September 1st of a given year, any player that has less than 65% of the
            projected points of the player with the most projected points will be given a super drop automatically. Only one super drop can be granted per player in this way, but it doesn't matter
            what the current date is when the condition is met, just that it is September 1st or greater. That means that if a player doesn't meet the criteria on September 1st, but then falls further
            behind, they will get the super drop once they become eligible for it.
          </p>
        </template>
      </collapseCard>
      <br />

      <h2><a id="trades">Trades</a></h2>
      <collapseCard>
        <template #header>How do trades work?</template>
        <template #body>
          Your league can chose whether or not to allow trades. If allowed, trades become available after the draft is over. The following rules apply:
          <ul>
            <li>Trades are always between 2 players.</li>
            <li>Each side can include any number of games as well as any budget ($) they have available.</li>
            <li>Each side must include something in the trade.</li>
            <li>Trades do not have to be "even", you can trade 2 games for one, or 5 games for 1 if you really want.</li>
            <li>Any game is tradeable, including counter picks and games that have already released.</li>
            <li>You can't trade anything outside of the current year (no trading for next year's draft picks, for example).</li>
          </ul>
          This is how the process of a trade works:
          <ul>
            <li>One player in your league proposes a trade with another player.</li>
            <li>That player can chose to accept or decline.</li>
            <li>If the trade is accepted, other players in the league get a chance to "vote" on whether or not they want the trade to be accepted.</li>
            <li>If the league agrees that the trade is a fair trade, then the league manager can "execute" the trade, and the games and money will change hands.</li>
          </ul>
          Note that the "votes" by the league members are not "votes" in the democracy sense. League managers should weigh them when deciding what to do with the trade, but ultimately, they have full
          control.
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>What is a "fair trade"?</template>
        <template #body>
          When a trade in your league is under consideration, you should ask "is this trade reasonable?" A reasonable trade is beneficial to both parties. The following situations are cases where
          trades should not be approved:
          <ul>
            <li>A player is already losing and has agreed to a bad trade to help the other player beat a third player not involved in the trade.</li>
            <li>A player is taking a bad trade this year with the understanding that they will be 'paid back' next year.</li>
            <li>You suspect that there is some other "shady deal" going on, such as real money being exchanged behind the scenes.</li>
          </ul>
          However, you should
          <em>not</em>
          be considering whether or not this trade is good for
          <em>you.</em>
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>Can you trade for a game that you have already counter picked?</template>
        <template #body>
          The system will let you do it. This would mean you would have a game as both a standard game and a counter pick, which is guarunteed to get you exactly 0 points no matter what. Your league
          might want to decide that this isn't allowed in your league though.
        </template>
      </collapseCard>

      <h2><a id="scoring">Scoring</a></h2>
      <collapseCard>
        <template #header>How does scoring work?</template>
        <template #body>
          <p>
            A game's score is based on it’s score from
            <a href="https://opencritic.com/" target="_blank">
              opencritic.com
              <font-awesome-icon icon="external-link-alt" size="sm" />
            </a>
            . Your team will gain or lose points based upon how well your game reviews. Only critic reviews are taken into account. For every point above a 70 on OpenCritic, your game gets you one
            point. For every point below 70, you lose one point. If a game doesn’t release in the year you are playing, it is treated as getting a 70, and therefore 0 points.
          </p>

          <p>
            However, there are special rules to make certain points worth more or less. If a game scores above a 90, every point above 90 is worth two points. So if a game gets a 94, you get 20 points
            for 70 through 90, and 8 points for 90 through 94, for a total of 28 points for the 94. However, starting in 2023, these "bonus" points can be turned off if you wish.
          </p>

          Another special rule is that there are diminishing returns on how much a very bad game hurts you. Each point from 60 through 70 loses you one point, but every 10 point range below that, each
          point is worth half of the range above it. I realize that sounds complicated, but I will provide examples below. The effect is that you can never lose more than 20 points, no matter how bad
          a game is.
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>What is a “counter pick”?</template>
        <template #body>
          After everyone has drafted their roster of games, the players will draft their counter picks. A counter pick is a game from someone else’s roster, but instead of getting points for it, you
          LOSE points for it. The goal is to choose a game from someone else’s team that you think will be bad, or one that you think won't even release.
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>If I counter pick a game that scores below a 70, will I gain points?</template>
        <template #body>Yes.</template>
      </collapseCard>
      <collapseCard>
        <template #header>If I counter pick a game that doesn't come out, what do I get?</template>
        <template #body>Both players get 0 points in this case. This should be considered a good result for the counter picker and a bad result for the person who actually chose the game.</template>
      </collapseCard>
      <collapseCard>
        <template #header>Can I see some scoring examples?</template>
        <template #body>
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
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>What order are counter picks chosen in?</template>
        <template #body>
          Counter picks are chosen in a snake draft - just like the regular games. However, the player who went LAST in the regular draft always goes first when it comes time to draft counter picks.
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>What's a manual score?</template>
        <template #body>
          If for some reason you don't agree with my methodologies, you can choose to manually assign a score. For example, if a game is released but never appears on OpenCritic, you could choose to
          add manual score from another site - hopefully one you decided on ahead of time. Or, if you think you should be able to draft specific episodes of an episodic series, you'll have to manually
          score those as well. See the
          <a href="#eligibility">eligibility</a>
          section for more on episodic games.
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>My game has a critic score but no points. What gives?</template>
        <template #body>
          The game needs to release before you get the points. So if reviews come out 3 days before the game, you will see a critic score but no points for a few days. The only time this will actually
          make a difference is if a game comes out in early January but has reviews come out in late December. In that case, you won't get the points for the year, because the game didn't come out.
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>I see a score on OpenCritic but not in my league. Why?</template>
        <template #body>
          My database probably hasn't refreshed yet. That happens every two hours. I don't want to hit OpenCritic's API more frequently than I have to, as doing so involves a bunch of rapid requests.
          Additionally, I have to manually enter the "OpenCritic ID" for every game in the master games list. OpenCritic doesn't usually add a game until it is close to release, so I need to check
          frequently to see if they have added new games. If you notice a particular game that is missing a score, feel free to let me know. If a score is just out of sync, it'll be fixed soon.
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>A game is linked to an Open Critic page but there is no score. Why?</template>
        <template #body>Open Critic only generates and displays an average score once there are at least three "Top Critic" reviews. This is the score that Fantasy Critic uses.</template>
      </collapseCard>
      <collapseCard>
        <template #header>One of my games was released but never got enough reviews to have a score on Open Critic. Can I drop it?</template>
        <template #body>
          By default, no. There are some games that come with an inherent risk of not getting enough reviews. For example, obscure indie games, games that have been in early access for a long time,
          free to play games, and free DLC for already released games. Judging this risk is part of selecting your games. You league is free to make a "house rule" and allow games in this situation to
          be dropped, if you so choose.
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>When are critic scores locked in?</template>
        <template #body>
          At the very end of the year. Usually scores don't change after the first few weeks a game has been out, but there's no reason to "lock" scores until it's time to determine the winner.
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>How do projected points work?</template>
        <template #body>
          The projected points system uses an algorithm to predict what score an individual game will get using a linear regression algorithm based on popularity of games on the site versus the score
          the game ultimately gets. Additionally, any empty slots will be given an estimated point value as well, as it is assumed that you will eventually fill those slots.
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>What's the "Pre-Release projected points"?</template>
        <template #body>This is the last "projected points" values before a game was reviewed. The idea is to compare what a game was projected to get vs. what it actually got.</template>
      </collapseCard>
      <br />

      <h2><a id="eligibility">Eligibility</a></h2>
      <collapseCard>
        <template #header>How does the "tags" system work?</template>
        <template #body>
          <p>
            All games have one or more "tags", such as "Remake", "New Game", or "Yearly Installment". The league manager can choose what tags are and are not allowed. Mouse over any tag to see a
            description of it.
          </p>

          <div v-if="allTags">
            <span v-for="tag in allTags" :key="tag.name">
              <masterGameTagBadge :tag-name="tag.name"></masterGameTagBadge>
            </span>
          </div>
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>How do you decide what kind of a "remake" a game is?</template>
        <template #body>
          <div v-if="allTags">
            <p>There are several "remake" tags, and they go in this order, from "most remade" to "least remade":</p>

            <masterGameTagBadge tag-name="Reimagining"></masterGameTagBadge>
            &gt;
            <masterGameTagBadge tag-name="Remake"></masterGameTagBadge>
            &gt;
            <masterGameTagBadge tag-name="PartialRemake"></masterGameTagBadge>
            &gt;
            <masterGameTagBadge tag-name="Remaster"></masterGameTagBadge>

            <p>
              There is also
              <masterGameTagBadge tag-name="DirectorsCut"></masterGameTagBadge>
              , which is somewhere around
              <masterGameTagBadge tag-name="PartialRemake"></masterGameTagBadge>
              and
              <masterGameTagBadge tag-name="Remaster"></masterGameTagBadge>
              , but it's hard to say for sure what order it goes in.
            </p>
            <p>
              As for how we decide what a game is, we judge where a game falls based on the information available before release. If the game has a close comparison, that will be used as a precedent.
              For example, when deciding how to classify 'Pokémon Brilliant Diamond and Shining Pearl', we considered what the prior Pokemon remakes would be classified as. When things aren't entirely
              clear, we will generally lean on "more remade" rather than "less remade".
            </p>
            <p>
              We have a
              <a href="https://tiermaker.com/list/video-games/fantasy-critic-remake-levels-585753/1752747" target="_blank">
                tier list
                <font-awesome-icon icon="external-link-alt" />
              </a>
              lays out a lot of examples, and we use it to guide our decisions.
            </p>
            <p>If you want to get involved in these making these decisions, we talk about this frequently on our Discord.</p>
            <a href="https://discord.gg/dNa7DD3" target="_blank">
              <font-awesome-icon :icon="['fab', 'discord']" size="lg" class="discord-icon" />
              Join our Server
            </a>
          </div>
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>I don't agree with your tags! My league allows "remakes" but "Tony Hawk 1 + 2" is really just a remaster. It shouldn't be allowed!</template>
        <template #body>
          It's up to your league and ultimately up to your league manager. League managers can override the eligibility rules and assign the game you want to your team during the draft. Additionally,
          outside of the draft, the league manager can manually 'allow' or 'ban' a specific game, which either allows or blocks players from bidding on the game. Whether you are making these
          exemptions at the draft or during the year, your league should agree on whether or not they want to allow the exception.
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>How do episodic games work?</template>
        <template #body>
          If no game from an episodic series has come out, you can select the game as a whole. If episodes have already come out, then the game will not be eligible. If you do have an episodic game on
          your roster, and only some of the episodes come out, then we use the average critic score of the released episodes. If the entire game has been released, the game usually receives reviews
          for the game as whole, and we use that score.
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>How do early access games work?</template>
        <template #body>
          <p>
            There are two tags related to early access games. You can use them to chooses to ban games from being drafted/picked up if they are already playable in early access, or, alternatively, if
            they are even PLANNED for early access. It's important to note that games will not recieve points until they are "fully released", meaning games do not get points while still in early
            access. This is true even if they get reviews while in early access, which is rare.
          </p>
          <p>
            However, new for 2023, leagues can choose to enable "Only Needs Score" mode, which would mean that an early access game that gets reviews and an OpenCritic score
            <em>would</em>
            count for points.
          </p>
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>Why can't I ban 'Planned for Early Access' without also banning 'Currently in Early Access'?</template>
        <template #body>
          This used to be allowed, but truthfully never should have been. Consider this scenerio:
          <ul>
            <li>A game is announced on Feburary 5th, 2022. It will go into early access on March 3rd, 2022, and have a full release on October 10th, 2022.</li>
            <li>Let's say you want to allow both 'Planned for Early Access' and 'Currently in Early Access'.</li>
            <li class="nested-list">
              <ul>
                <li>
                  On Feburary 5th, 2022, the game becomes draftable/biddable. It remains draftable/biddable up until either it's full release on October 10th, 2022, or when it gets reviews, whichever
                  comes first.
                </li>
                <li>Most of the time, a game will not get critic reviews while in early access. Howeever, it does happen sometimes, and that does mean you can't draft or bid on the game.</li>
              </ul>
            </li>
            <li>Now consider what happens if you BAN both 'Planned for Early Access' and 'Currently in Early Access'.</li>
            <li class="nested-list">
              <ul>
                <li>On Feburary 5th, 2022, the game is immediately 'Planned for Early Access', so you cannot ever bid on it or draft it.</li>
              </ul>
            </li>
            <li>Now consider what happens if you allow 'Planned for Early Access', but not 'Currently in Early Access'.</li>
            <li class="nested-list">
              <ul>
                <li>On Feburary 5th, 2022, the game becomes draftable/biddable. It remains draftable/biddable up until either it enters early access on March 3rd, 2022.</li>
              </ul>
            </li>
            <li>Finally, consider what happens if you allow 'Currently in Early Access', but not 'Planned for Early Access'.</li>
            <li class="nested-list">
              <ul>
                <li>
                  On Feburary 5th, 2022, the game is immediately 'Planned for Early Access', so you cannot bid on it or draft it. However, when the game enters early access on March 3rd, 2022, now it
                  is 'Currently in Early access', which IS allowed, so now you CAN bid on it/draft it up until it's full release on October 10th, 2022, or when it gets reviews, whichever comes first.
                </li>
                <li>We believe that this setup should not be allowed, due to it's confusing nature. It's also a nightmare to code for.</li>
              </ul>
            </li>
          </ul>
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>Why can't I ban 'Will Release Internationally First' without also banning 'Released Internationally'?</template>
        <template #body>See the above answer about 'Planned for Early Access' and 'Currently in Early Access', because the same logic applies.</template>
      </collapseCard>
      <collapseCard>
        <template #header>How are "re-releases" of games that were never released in North America handled?</template>
        <template #body>
          <div class="alert alert-info">
            Note: This rule has been changed for the 2023 season. The below text reflects the new rule, and games that came out in 2022 and earlier may be classified differently than this text would
            suggest.
          </div>
          <p>
            This one is complicated. For example, if a game came out in Japan, but not North America, and then some time later, the game is updated with a new release in Japan and
            <em>that</em>
            release comes out in North America, the tags on the "updated release" will reflect the fact that "this version" of the game has not released anywhere yet, and therefore will not have a
            "Released Internationally" tag.
          </p>

          <p>Here are some examples for clarity:</p>

          <table class="table table-striped table-bordered table-responsive-sm">
            <thead>
              <tr class="bg-primary">
                <th>Game Name</th>
                <th>Tags</th>
                <th>Notes</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td>Rune Factory 5</td>
                <td>
                  <masterGameTagBadge tag-name="NewGame"></masterGameTagBadge>
                  <masterGameTagBadge tag-name="ReleasedInternationally"></masterGameTagBadge>
                </td>
                <td>
                  This example is here as a baseline, as this is simply a game that was released in Japan before it was released in North America. The version of the game released in North America is
                  basically the same as the one released in Japan, save for the localization. It was also released on the same platform in both regions.
                </td>
              </tr>
              <tr>
                <td>The Legend of Heroes: Trails from Zero</td>
                <td>
                  <masterGameTagBadge tag-name="Remaster"></masterGameTagBadge>
                  <masterGameTagBadge tag-name="ReleasedInternationally"></masterGameTagBadge>
                </td>
                <td>
                  This game has been released a number of times:
                  <ul>
                    <li>On PSP in Japan in 2010</li>
                    <li>On PC in China in 2011</li>
                    <li>On PS Vita in Japan in 2012</li>
                    <li>On PC in Japan in 2013</li>
                    <li>On PS4 in Japan in 2020</li>
                    <li>On Switch in Japan in 2021</li>
                  </ul>

                  It will release world wide, including in North America, in Fall of 2022 on PC, PS4, and Switch. This world wide release can be considered a remaster of the original PSP game.
                  However, this version is already released elsewhere, it is consider both a remaster and "released internationally".
                </td>
              </tr>
              <tr>
                <td>Earthbound Beginnings</td>
                <td>
                  <masterGameTagBadge tag-name="Port"></masterGameTagBadge>
                </td>
                <td>This is a port of Mother 1 for the Famicom to the Wii U.</td>
              </tr>
              <tr>
                <td>River City Girls Zero</td>
                <td>
                  <masterGameTagBadge tag-name="Remaster"></masterGameTagBadge>
                </td>
                <td>This game was originally released on the Super Famicom in 1994, now remastered for modern platforms.</td>
              </tr>
              <tr>
                <td>The Great Ace Attorney Chronicles</td>
                <td>
                  <masterGameTagBadge tag-name="Remaster"></masterGameTagBadge>
                </td>
                <td>This game is a compilation of two Nintendo 3DS games that never came out in North America, remastered for Nintendo Switch.</td>
              </tr>
              <tr>
                <td>Bravely Default</td>
                <td>
                  <masterGameTagBadge tag-name="DirectorsCut"></masterGameTagBadge>
                  <masterGameTagBadge tag-name="ReleasedInternationally"></masterGameTagBadge>
                </td>
                <td>
                  Bravely Default was originally a Japan-only 3DS game called "Bravely Default: Flying Fairy", released in 2012. In 2013, an expanded version was released, again only in Japan, called
                  "Bravely Default: For The Sequel". Then, in 2014, "Bravely Default: For The Sequel" was released in North America as "Bravely Default". This version is best described as a
                  "Director's Cut" of the original.
                </td>
              </tr>
              <tr>
                <td>Famicom Detective Club</td>
                <td>
                  <masterGameTagBadge tag-name="Remake"></masterGameTagBadge>
                </td>
                <td>This was originally released as two seperate Famicom games, never released in North America. It's a fairly substantial remake over the original.</td>
              </tr>
              <tr>
                <td>Live A Live</td>
                <td>
                  <masterGameTagBadge tag-name="Remake"></masterGameTagBadge>
                </td>
                <td>This was originally a Super Famicom game, never released in North America.</td>
              </tr>
            </tbody>
          </table>
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>What's the definition of a 'New Gaming Franchise'?</template>
        <template #body>
          <p>I admit this is tricky, so here's a list of criteria that we consider:</p>

          <h6>
            Signs a game
            <em>is</em>
            a 'New Gaming Franchise':
          </h6>
          <ul>
            <li>Is this game an obvious, clear as day new IP?</li>
            <li class="nested-list">
              <ul>
                <li>Demon's Souls (2009), Celeste, Death's Door</li>
              </ul>
            </li>
            <li>Is this game a 'spiritual successor' but not the same fictional universe?</li>
            <li class="nested-list">
              <ul>
                <li>Elden Ring, Back 4 Blood, Yooka-Laylee</li>
              </ul>
            </li>
            <li>Is this game based on a property that has never been a video game before?</li>
            <li class="nested-list">
              <ul>
                <li>Cyberpunk 2077, Metro 2033</li>
              </ul>
            </li>
            <li>
              Is this game based on a public domain IP, and
              <em>this adaptation</em>
              is new?
            </li>
            <li class="nested-list">
              <ul>
                <li>Sherlock Holmes: The Mystery of the Mummy (2002), The Sinking City</li>
              </ul>
            </li>
            <li>Is this game 'very on brand' for a developer but nonetheless not a sequel?</li>
            <li class="nested-list">
              <ul>
                <li>Triangle Strategy</li>
                <li>Rock Band</li>
              </ul>
            </li>
            <li>Is this a real world simulation with licensed elements, and the developer/publisher has not created a simulation of this type before?</li>
            <li class="nested-list">
              <ul>
                <li>Skate (2005)</li>
                <li>F1 Management 2022</li>
                <li>NFL 2K (1999)</li>
              </ul>
            </li>
            <li>
              Does this game have "easter eggs" or guest characters that are from an existing game, but these are not the focus of the game, and this is otherwise clearly a New Gaming Franchise?
            </li>
            <li class="nested-list">
              <ul>
                <li>Brawlhalla</li>
                <li>Yooka-Laylee (Shovel Knight makes an appearance)</li>
                <li>The Last of Us (Teased in Uncharted 3)</li>
              </ul>
            </li>
          </ul>

          <h6>
            Signs a game
            <em>is not</em>
            a 'New Gaming Franchise'
          </h6>
          <ul>
            <li>Is this game better defined as a sequel, spinoff, or reboot?</li>
            <li class="nested-list">
              <ul>
                <li>Tomb Raider (2013), Prince of Persia (2008), Gears Tactics, Project L (League of Legends fighting game)</li>
              </ul>
            </li>
            <li>
              Is this game based on a fictional IP, that is
              <em>not</em>
              in the public domain, that IS represented in video games?
            </li>
            <li class="nested-list">
              <ul>
                <li>Spider Man (2018), Gotham Knights, Wonder Woman</li>
              </ul>
            </li>
            <li>Is this a real world simulation with licensed elements, and a direct follow up to another game?</li>
            <li class="nested-list">
              <ul>
                <li>The WWE games switched publishers in 2014 and switched developers in 2020, but in neither case are they a New Gaming Franchise.</li>
                <li>Blitz: The League (2005) is a follow up to NFL BLitz Pro (2003), but without the NFL license.</li>
              </ul>
            </li>
          </ul>
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>What is a 'Special Game Slot'?</template>
        <template #body>
          <p>
            New for 2022, special roster slots allow your league to specify a requirement of certain game types in each roster, including games that are banned in your normal league rules. For example
            even if your league doesn’t allow yearly installments, you can require each publisher to have one yearly installment on their roster. This slot will only give points for games that meet
            its designated requirements.
          </p>

          <p>
            A good way to think of this is like "positions" in Fantasy Football. You don't just draft 11 "players", you have quarterbacks, running backs, wide recievers, etc, each of which have
            designated slots. In Fantasy Critic, these "positions" are the tags, so you can have "Remake" slot, a "Currently in Early Access" slot, and a "Virtual Reality" slot, if you so desire.
          </p>
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>How do I move games into a special slot?</template>
        <template #body>
          They system should assign a game to the best slot when drafted or picked up. However, if you do need to move a game in your roster, this can be done at the My Publisher Details page, found
          under the Player Actions tab.
        </template>
      </collapseCard>
      <collapseCard>
        <template #header>What happens if a game is in a slot it is not eligible for?</template>
        <template #body>
          This can happen in any league, whether or not the league uses special slots, for a few different reasons:
          <ul>
            <li>A game's tags changed after a correction was made.</li>
            <li>The player picked up a game and there were no eligible slots it could fit in.</li>
            <li>The player intentionally moved a game into a slot it's not eligble for. This used to be possible but no longer is.</li>
          </ul>

          <p>
            A game in a slot it isn't eligible recieves no points, unless the game recieved negative points, then the points still count. A game's projected points also always count towards
            tiebreakers on bids, to prevent players from trying to game the tiebreaker system.
          </p>

          <p>
            Generally speaking, players should always be putting their games into slots they are eligible for. Intentionally keeping games in ineligible slots in order to free up 'more valuable' slots
            is outside of the spirit of Fantasy Critic.
          </p>

          Some reasonable options are:
          <ul>
            <li>The player can reorganize their games so everything is eligible.</li>
            <li>The league manager can override a game's tags if the league disagrees with the tags the site decided.</li>
            <li>The player can drop the game (depending on the league settings).</li>
            <li>The league could decide to give the player a "free drop" regardless of the league settings if they decide that this game should not be eligible.</li>
            <li>The player could trade the game to another player.</li>
          </ul>
        </template>
      </collapseCard>
      <br />

      <h2><a id="winning">Winning</a></h2>
      <collapseCard>
        <template #header>What do I get if I win?</template>
        <template #body>
          That’s totally up to your league. This site doesn’t provide any prizes. Look to fantasy sports for advice on how to structure any winnings you want to play for. My personal favorite is to
          end the year with a group dinner where the winner doesn't pay, and then do the draft for the next year right after that.
        </template>
      </collapseCard>
    </div>
  </div>
</template>
<script>
import CollapseCard from '@/components/collapseCard.vue';
import MasterGameTagBadge from '@/components/masterGameTagBadge.vue';

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
