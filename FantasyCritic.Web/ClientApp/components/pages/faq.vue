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
        <li><a href="#scoring">Scoring</a></li>
        <li><a href="#eligibility">Eligibility</a></li>
        <li><a href="#winning">Winning</a></li>
      </ul>
      <hr />

      <h2><a name="general">General</a></h2>
      <h3>Something about the site is broken!</h3>
      <p class="text-well">
        Sounds likely. We’re just getting started here at Fantasy Critic and there are bound to be issues.
        Honestly, most issues can be fixed by refreshing the page.
        Beyond that, the best option is to open an issue on <a href="https://github.com/SteveF92/FantasyCritic" target="_blank">GitHub <font-awesome-icon icon="external-link-alt" size="sm" /></a>.
        Contacting me on Twitter or Reddit is also an option.
      </p>
      <h3>Something goes wrong when I try to confirm my email address.</h3>
      <p class="text-well">
        I've heard this from a few people now, and I haven't been able to identify the cause. It seems to be inconsistent as most people have no problems. I do have a list of potential solutions, though.
        <ol>
          <li>Try pressing CTRL+F5 when you are on the site and then clicking the link in the email again.</li>
          <li>Try an incognito window.</li>
          <li>Login first then try clicking the link.</li>
          <li>Logout first then try clicking the link.</li>
          <li>Try resending the email by clicking on "Manage Account" in the dropdown that comes up when you click your username in the top right, and using that link.</li>
          <li>Try a combination of these things.</li>
          <li>If all else fails, contact me.</li>
        </ol>
      </p>
      <h3>How can I follow updates for the site?</h3>
      <p class="text-well">
        Follow us on twitter <a href="https://twitter.com/fantasy_critic" target="_blank">@fantasy_critic <font-awesome-icon icon="external-link-alt" size="sm" /></a> and join our <a href="https://www.reddit.com/r/fantasycritic/" target="_blank">subreddit <font-awesome-icon icon="external-link-alt" size="sm" /></a>.
      </p>
      <h3>Why should I play FantasyCritic?</h3>
      <p class="text-well">
        It’s a great catalyst for discussing video games with your friends, and gets everyone invested in checking out the year’s best video games.
      </p>
      <h3>Does this cost money?</h3>
      <p class="text-well">
        No. It’s totally free.
      </p>
      <h3>Wait really? What’s the catch?</h3>
      <p class="text-well">
        There isn’t one. For now, this is a passion project for me and I’m comfortable eating the cost of servers and advertising.
        In the future, it is possible that I will consider adding advertising or opening a Patreon, but I’m never going to charge people to play.
      </p>
      <br />

      <h2><a name="league-creation">League Creation</a></h2>
      <h3>How many players can be in a league?</h3>
      <p class="text-well">
        Anywhere from 2 to 14. Somewhere between 4 and 8 is probably optimal, but you’ll do fine anywhere in that range.
      </p>
      <h3>How long does a league last?</h3>
      <p class="text-well">
        Each calendar year is one “game” of Fantasy Critic. Everyone starts fresh with a new roster at the start of each new year, but you are free to keep playing
        in the same league with the same people year after year.
      </p>
      <h3>What does the league manager do?</h3>
      <p class="text-well">
        Similar to fantasy football, the league manager is in charge of inviting players, deciding on the settings for the league, and making administrative decisions
        throughout the year.
      </p>
      <h3>What settings can be chosen by the league manager?</h3>
      <p class="text-well">
        <ul>
          <li>Amount of games per team.</li>
          <li>How many games to draft vs how many games to leave as mid-year pickups.</li>
          <li>Number of counter picks to draft per team.</li>
          <li>The league’s “eligibility level” (more on this below).</li>
          <li>Whether or not to allow yearly installments, early access games, free to play games, and games already released in other regions.</li>
          <li>Whether or not to allow expansion packs/DLC.</li>
        </ul>
      </p>
      <h3>How many games should I allow per team?</h3>
      <p class="text-well">
        The league creation page will guide you through this, but you should shoot for about 50 games in your whole league. This means that bigger leagues should probably
        have smaller roster sizes, but that’s up to you.
      </p>
      <h3>How many games should I set as “Games to draft”?</h3>
      <p class="text-well">
        The standard is that half of your games should be draft games. If you are in a 6 person league, the default is 12 games per player, 6 of them being selected at the draft,
        and 1 counter pick. That leaves you with 6 pickup games per player.
      </p>
      <h3>The site says I need to create a "Publisher", what does that mean?</h3>
      <p class="text-well">
        So the "backstory" of Fantasy Critic is that you are the CEO of a video game publisher, and you need to decide which games currently in development are worth publishing. Your "Publisher" is
        just your "team" name. It's just a flavor thing. You can make it something serious or a big joke. Doesn't matter at all.
      </p>
      <h3>What is a 'public league'?</h3>
      <p class="text-well">
        If you set your league to public, you allow people who are not in the league to view it. It's also added to a list of all public leagues for ease of finding popular public leagues.
        However, you still need an invite to join a public league. This setting only affects who can view a league, not who can join it.
      </p>
      <h3>What is a 'test league'?</h3>
      <p class="text-well">
        Test leagues function exactly the same as normal leagues, except they don't affect the overall stats for games in the system, such as hype factor.
        This is to prevent people from manipulating the stats.
      </p>
      <h3>My league was set to a test league and it won't let me change it back!</h3>
      <p class="text-well">
        League Managers can set a league as test league whenever they want, but cannot change a test league into a regular league. You will need to contact me if you want me to change your league.
        If I suspect that a league doesn't represent an honest effort by at least two seperate people, I reserve the right to set it as a test league.
      </p>
      <h3>How do I delete a league?</h3>
      <p class="text-well">
        As of now, you'll need to contact me. Renaming your league to something like "delete me!" and sending me your league ID (it's what appears afer 'league/' and before '/{year}' in the URL) helps.
      </p>
      <br />

      <h2><a name="drafting">Drafting</a></h2>
      <h3>How does the draft work?</h3>
      <p class="text-well">
        Your league will decide on the order that the teams will draft in. Then the league manager will decide when to begin the draft. Once it has started, the teams will
        choose games in the order that was chosen, snake style, until all teams have filled all of their draft game slots.
      </p>
      <h3>What's a snake draft?</h3>
      <p class="text-well">
        A snake draft means that the person who goes last in a round of drafting, goes first in the next round. So with 4 teams the first 9 games
        would go Team A, B, C, D, D, C, B, A, A.
      </p>
      <h3>During the draft, is there a time limit to picking games for each player?</h3>
      <p class="text-well">
        No, but leagues are encouraged to set a time to do their draft and have all players committed to completing it in one sitting. Even better, have all players in
        the same room or on a group video chat, to facilitate discussion of the games being selected.
      </p>
      <h3>Am I picking games from a list or can I pick anything?</h3>
      <p class="text-well">
        Both, actually. I try to maintain a fairly inclusive list of upcoming games, which I call my "master game" list. If your game is in this list, it will show
        up when you search for it when drafting. If it doesn't, you can draft a "unlisted" game. I will frequently update the list of "master games", and the league manager
        can link a "unlisted" game to a "master game" at any point after the draft is completed, at no penalty to the player. However, if a game never recieves a score from
        OpenCritic, you will never get any points from it. They are fairly inclusive, but pick an extremely obscure game at your own risk.
      </p>
      <h3>How do the rosters work?</h3>
      <p class="text-well">
        Each roster is made up of a number of “draft games”, “pickup games”, and “counter picks”. Draft games and pickup games are the games that you get points for.
        Any one game can only be on one roster within a single league. Counter picks work a bit different and are described below.
      </p>
      <h3>What's this "Hype Factor" thing I see when I click on a game?</h3>
      <p class="text-well">
        "Hype Factor" is a measure of how highly anticipated a game is. It's a 0-100 value, with 100 being the best, derived from the percentage of leagues that draft a game, and what its average draft position is in those leagues.
        Only leagues that could have drafted a game, based on it's settings and the date when the league draft are counted in the percentage.
      </p>
      <br />

      <h2><a name="bidding-system">Bidding System</a></h2>
      <h3>What is a “draft game” vs a “pickup game”?</h3>
      <p class="text-well">
        Draft games are chosen during the draft - turn by turn at the start of the year. Pickup games are chosen over the course of year (after the draft) by bidding on them.
      </p>
      <h3>How does the bidding system work?</h3>
      <p class="text-well">
        If you’re familiar with Free Agency Acquisition Budget (FAAB) rules in fantasy football, it works like that. Essentially, any player that is interested in a game will place a secret "bid" for the game,
        using some of their available budget. Every player starts the year with $100 of budget. Then, on Monday evenings at 8PM Eastern, the system will process those bids. The top bidder for any given game
        will recieve the game and spend the money that they bid. The other players keep their money but do not get the game.
      </p>
      <h3>What games are eligible?</h3>
      <p class="text-well">
        Same rules as during the draft. The system will warn you if you try to bid on a game that has already been released or already has reviews, but be aware that the game also must be eligible at the times the bid process,
        not just when you make the bid. For example, if you bid on a game on Thursday, and reviews come out on Saturday, then the bid will fail come Monday.
      </p>
      <h3>Will I still lose my money even if no one else is bidding on a game?</h3>
      <p class="text-well">
        Yes.
      </p>
      <h3>Can I bid zero dollars on a game?</h3>
      <p class="text-well">
        Yes, but you will be outbid by anyone who bids at least a dollar.
      </p>
      <h3>Can I bid on a game that doesn't come up when I search for games?</h3>
      <p class="text-well">
        Unfortunately no. To bid on a game, it must be in the master database. I try to stay on top of upcoming releases, but if a game that you want is missing,
        you can use the <router-link :to="{ name: 'masterGameRequest' }">master game request form</router-link>.
      </p>
      <h3>When are bids processed?</h3>
      <p class="text-well">
        Bids are processed on Monday evenings at 8PM Eastern (New York City time). While bids are processing, you will not be able to:
        <ul>
          <li>Make bids</li>
          <li>Cancel bids</li>
          <li>Add/Remove Publisher Games post draft (as a manager)</li>
          <li>Associate Publisher Games</li>
        </ul>
        The processing doesn't take long (no longer than 20 minutes, and probably much less). I only need to block these actions because if they happen during bid processing, it would cause conflicts with my algorithms.
      </p>
      <h3>How are ties handled on bids?</h3>
      <p class="text-well">
        If two or more players bid the same amount for the same game, the tie goes to the player with less projected points, and then if THAT's a tie, the game goes to the person who drafted later in
        the draft. My hope is that we rarely have to go to that level of tiebreaker.
      </p>
      <br />

      <h2><a name="scoring">Scoring</a></h2>
      <h3>How does scoring work?</h3>
      <p class="text-well">
        A game's score is based on it’s score from <a href="https://opencritic.com/" target="_blank">opencritic.com <font-awesome-icon icon="external-link-alt" size="sm" /></a>. Your team will gain or lose points based upon how well your game reviews.
        Only critic reviews are taken into account. For every point above a 70 on OpenCritic, your game gets you one point. For every point below 70,
        you lose one point. However, to really separate the cream of the crop, every point above 90 gets you <em>two</em> points. If a game doesn’t release in the
        year you are playing, it is treated as getting a 70, and therefore 0 points.
      </p>
      <h3>What is a “counter pick”?</h3>
      <p class="text-well">
        After everyone has drafted their roster of games, the players will draft their counter picks. A counter pick is a game from someone else’s roster, but instead of
        getting points for it, you LOSE points for it. The goal is to choose a game from someone else’s team that you think will be bad, or one that you think won't even release.
      </p>
      <h3>If I counter pick a game that scores below a 70, will I gain points?</h3>
      <p class="text-well">
        Yes.
      </p>
      <h3>If I counter pick a game that doesn't come out, what do I get?</h3>
      <p class="text-well">
        Both players get 0 points in this case. This should be considered a good result for the counter picker and a bad result for the person who actually chose the game.
      </p>
      <h3>Scoring Examples</h3>
      <table class="table table-striped table-bordered">
        <thead>
          <tr class="bg-primary">
            <th>Game Name</th>
            <th>OpenCritic Score</th>
            <th>Fantasy Points</th>
            <th>Counter Pick Points</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td>Dragon Ball FighterZ</td>
            <td>86</td>
            <td>16</td>
            <td>-16</td>
          </tr>
          <tr>
            <td>Shadow of the Tomb Raider</td>
            <td>80</td>
            <td>10</td>
            <td>-10</td>
          </tr>
          <tr>
            <td>Fallout 76</td>
            <td>52</td>
            <td>-18</td>
            <td>18</td>
          </tr>
          <tr>
            <td>Red Dead Redemption 2</td>
            <td>97</td>
            <td>34</td>
            <td>-34</td>
          </tr>
          <tr>
            <td>Kingdom Hearts III</td>
            <td>Delayed to 2019</td>
            <td>0</td>
            <td>0</td>
          </tr>
        </tbody>
      </table>
      <h3>What order are counter picks chosen in?</h3>
      <p class="text-well">
        Counter picks are chosen in a snake draft - just like the regular games. However, the player who went LAST in the regular draft always goes first when it comes time to draft counter picks.
      </p>
      <h3>What's a manual score?</h3>
      <p class="text-well">
        If for some reason you don't agree with my methodologies, you can choose to manually assign a score. For example, if a game is released but never appears on OpenCritic, you could choose to add
        manual score from another site - hopefully one you decided on ahead of time. Or, if you think you should be able to draft specific episodes of an episodic series, you'll have to manually score
        those as well. See the <a href="#eligibility">eligibility</a> section for more on episodic games.
      </p>
      <h3>My game has a critic score but no points. What gives?</h3>
      <p class="text-well">
        The game needs to release before you get the points. So if reviews come out 3 days before the game, you will see a critic score but no points for a few days. The only time this will actually
        make a difference if a game comes out in early January but has reviews come out in late December. In that case, you won't get the points for the year, because the game didn't come out.
      </p>
      <h3>I see a score on OpenCritic but not in my league. Why?</h3>
      <p class="text-well">
        My database probably hasn't refreshed yet. That happens every two hours. I don't want to hit OpenCritic's API more frequently than I have to, as doing so
        involves a bunch of rapid requests. Additionally, I have to manually enter the "OpenCritic ID" for every game in the master games list.
        OpenCritic doesn't usually add a game until it is close to release, so I need to check frequently to see if they have added new games. If you notice a particular
        game that is missing a score, feel free to let me know. If a score is just out of sync, it'll be fixed soon.
      </p>
      <h3>I see a score on the site but not on OpenCritic? Why?</h3>
      <p class="text-well">
        This typically happens when a game is very new. OpenCritic doesn't display scores until there's a certain number of reviews available. However, if you retrieve scores via the API, it will give
        a result as soon as there is even one review. Since FantasyCritic uses the OpenCritic API, you will see a score here before OpenCritic has displayed it on their site. In these cases, the score
        is likely to change significantly as very few reviews are accounted for.
      </p>
      <h3>When are critic scores locked in?</h3>
      <p class="text-well">
        At the very end of the year. Usually scores don't change after the first few weeks a game has been out, but there's no reason to "lock" scores until it's time to determine the winner.
      </p>
      <br />

      <h2><a name="eligibility">Eligibility</a></h2>
      <h3>What is an “eligibility level”?</h3>
      <p class="text-well">
        When deciding on the settings for a league, the league manager must choose what games to allow. The spirit of the game is to make predictions.
        In keeping with that spirit, some games are simply too safe of a bet, and don’t create interesting discussion. All games are categorized (by me) into “eligibility levels”,
        ranging from the most acceptable in all leagues, to games allowable in only the most lenient leagues.
      </p>
      <h3>What are the different levels?</h3>
      <p class="text-well">
        <ul>
          <li>
            <h4>New Game</h4>
            A definitively new game.
            <h5>Examples</h5>
            <ul>
              <li>Spider Man (2018)</li>
              <li>Red Dead Redemption 2</li>
              <li>Detroit: Become Human</li>
              <li>Call of Duty: Black Ops 4</li>
              <li>Assassin's Creed Odyssey</li>
            </ul>
          </li>
          <li>
            <h4>Complete Remake</h4>
            A remake that radically overhauls the original gameplay.
            <h5>Examples</h5>
            <ul>
              <li>Silent Hill: Shattered Memories</li>
              <li>Resident Evil 2 Remake</li>
              <li>Final Fantasy VII Remake</li>
              <li>Metroid: Samus Returns</li>
              <li>Tomb Raider: Anniversary</li>
            </ul>
          </li>
          <li>
            <h4>Remake</h4>
            A remake that modernizes gameplay without fundamentally changing it.
            <h5>Examples</h5>
            <ul>
              <li>Pokemon Omega Ruby and Alpha Sapphire</li>
              <li>Fire Emblem: Shadow Dragon</li>
              <li>Mario & Luigi: Superstar Saga + Bowser's Minions</li>
            </ul>
          </li>
          <li>
            <h4>Partial Remake</h4>
            A game that adds some new features while largely playing the same as the original.
            <h5>Examples</h5>
            <ul>
              <li>Crash Bandicoot N. Sane Trilogy</li>
              <li>The Legend of Zelda: Ocarina of Time 3D</li>
              <li>The Legend of Zelda: The Windwaker HD</li>
              <li>Metroid Prime Trilogy</li>
              <li>Shadow of the Colossus (2018)</li>
            </ul>
          </li>
          <li>
            <h4>Remaster</h4>
            A re-release that updates graphics while changing little else.
            <h5>Examples</h5>
            <ul>
              <li>Halo: The Master Chief Collection</li>
              <li>Uncharted: The Nathan Drake Collection</li>
              <li>Gears of War: Ultimate Edition</li>
              <li>Call of Duty: Modern Warfare Remastered</li>
            </ul>
          </li>
          <li>
            <h4>Port</h4>
            A game that was not originally released on a particular platform that is ported to it without many changes.
            <h5>Examples</h5>
            <ul>
              <li>Doom - Nintendo Switch Edition</li>
              <li>Sonic Mania Plus</li>
              <li>Captain Toad: Treasure Tracker (Nintendo Switch)</li>
            </ul>
          </li>
        </ul>
      </p>
      <h3>
        I don't agree with your eligibility levels! My league is set to a maximum level of “Complete Remake” but I should be able
        to take the new 'Mario & Luigi: Bowser's Inside Story' on 3DS.
      </h3>
      <p class="text-well">
        It's up to your league and ultimately up to your league manager. League managers can override the eligibility rules and assign
        the game you want to your team during the draft. Additionally, outside of the draft, the league manager can manually 'allow' or 'ban'
        a specific game, which either allows or blocks players from bidding on the game. Whether you are making these exemptions at the draft or
        during the year, your league should agree on whether or not they want to allow the exception.
      </p>
      <h3>
        How do episodic games work?
      </h3>
      <p class="text-well">
        If no game from an episodic series has come out, you can select the game as a whole. If episodes have already come out, then the game will not be eligible.
        If you do have an episodic game on your roster, and only some of the episodes come out, then we use the average critic score of the released episodes.
        If the entire game has been released, the game usually receives reviews for the game as whole, and we use that score.
      </p>
      <h3>
        How do early access games work?
      </h3>
      <p class="text-well">
        If you check "Allow Early Access Games", then there's no restrictions on early access at all. If you don't check it, then once a game enters
        early access (as in, once you can play it), then it is no longer eligible. You never get points until reviews come in. That part, I don't decide.

        The idea of the rule is to prevent people from taking games that are playable now, despite not having review scores. It's not much of a prediction if you can go play it
        at the time you are picking it.
      </p>
      <br />

      <h2><a name="winning">Winning</a></h2>
      <h3>What do I get if I win?</h3>
      <p class="text-well">
        That’s totally up to your league. This site doesn’t provide any prizes. Look to fantasy sports for advice on how to structure any winnings you want to play for.
      </p>
    </div>
  </div>
</template>
