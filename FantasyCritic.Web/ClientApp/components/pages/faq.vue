<template>
  <div>
    <h2>Frequently Asked Questions</h2>
    <hr />
    <div class="col-lg-6 col-sm-12">
      <h4>Something about the site is broken!</h4>
      <p>
        Sounds likely. We’re just getting started here at Fantasy Critic and there are bound to be issues.
        The best option is to open an issue on <a href="https://github.com/SteveF92/FantasyCritic" target="_blank">GitHub <font-awesome-icon icon="external-link-alt" size="sm" /></a>.
        Contacting me on Twitter or Reddit is also an option.
      </p>
      <h4>How can I follow updates for the site?</h4>
      <p>
        Follow us on twitter <a href="https://twitter.com/fantasy_critic" target="_blank">@fantasy_critic <font-awesome-icon icon="external-link-alt" size="sm" /></a> and join our <a href="https://www.reddit.com/r/fantasycritic/" target="_blank">subreddit <font-awesome-icon icon="external-link-alt" size="sm" /></a>.
      </p>
      <h4>Why should I play FantasyCritic?</h4>
      <p>
        It’s a great catalyst for discussing video games with your friends, and gets everyone invested in checking out the year’s best video games.
      </p>
      <h4>Does this cost money?</h4>
      <p>
        No. It’s totally free.
      </p>
      <h4>Wait really? What’s the catch?</h4>
      <p>
        There isn’t one. For now, this is a passion project for me and I’m comfortable eating the cost of servers and advertising.
        In the future, it is possible that I will consider adding advertising or opening a Patreon, but I’m never going to charge people to play.
      </p>
      <h4>How many players can be in a league?</h4>
      <p>
        Anywhere from 2 to 14. Somewhere between 4 and 8 is probably optimal, but you’ll do fine anywhere in that range.
      </p>
      <h4>How long does a league last?</h4>
      <p>
        Each calendar year is one “game” of Fantasy Critic. Everyone starts fresh with a new roster at the start of each new year, but you are free to keep playing
        in the same league with the same people year after year.
      </p>
      <h4>What does the league manager do?</h4>
      <p>
        Similar to fantasy football, the league manager is in charge of inviting players, deciding on the settings for the league, and making administrative decisions
        throughout the year.
      </p>
      <h4>What settings can be chosen by the league manager?</h4>
      <p>
        <ul>
          <li>Amount of games per team.</li>
          <li>How many games to draft vs how many games to leave as mid-year pickups.</li>
          <li>Number of counter picks to draft per team.</li>
          <li>The league’s “eligibility level” (more on this below).</li>
          <li>Whether or not to allow yearly installments and early access games.</li>
        </ul>
      </p>
      <h4>How many games should I allow per team?</h4>
      <p>
        The league creation page will guide you through this, but you should shoot for about 50 games in your whole league. This means that bigger leagues should probably
        have smaller roster sizes, but that’s up to you.
      </p>
      <h4>How many games should I set as “Games to draft”? </h4>
      <p>
        The standard is that ⅔ of your games should be draft games. If you are in a 6 person league, the default is 9 games per player, 6 of them being selected at the draft,
        and 1 counter pick. That leaves you with 3 pickup games per team.
      </p>
      <h4>How does the draft work?</h4>
      <p>
        Your league will decide on the order that the teams will draft in. Then the league manager will decide when to begin the draft. Once it has started, the teams will
        choose games in the order that was chosen, snake style, until all teams have filled all of their draft game slots.
      </p>
      <h4>Whats a snake draft?</h4>
      <p>
        A snake draft means that the person who goes last in a round of drafting, goes first in the next round. So with 4 teams the first 9 games
        would go Team A, B, C, D, D, C, B, A, A.
      </p>
      <h4>During the draft, is there a time limit to picking games for each player?</h4>
      <p>
        No, but leagues are encouraged to set a time to do their draft and have all players committed to completing it in one sitting. Even better, have all players in
        the same room or on a group video chat, to facilitate discussion of the games being selected.
      </p>
      <h4>How do the rosters work?	</h4>
      <p>
        Each roster is made up of a number of “draft games”, “pickup games”, and “counter picks”. Draft games and pickup games are the games that you get points for.
        Any one game can only be on one roster within a single league. Counter picks work a bit different and are described below.
      </p>
      <h4>What is a “draft game” vs a “pickup game”?</h4>
      <p>
        Draft games are chosen during the draft - turn by turn at the start of the year. Pickup games are chosen over the course of year (after the draft) by bidding on them.
      </p>
      <h4>How does the bidding system work?</h4>
      <p>
        If you’re familiar with Free Agency Acquisition Budget (FAAB) rules in fantasy football, it works like that.
      </p>
      <h4>What is a “counter pick”?</h4>
      <p>
        After everyone has drafted their roster of games, the players will draft their counter picks. A counter pick is a game from someone else’s roster, but instead of
        getting points for it, you LOSE points for it. The goal is to choose a game from someone else’s team that you think will be bad, or one that you think won't even release.
      </p>
      <h4>How does scoring work?</h4>
      <p>
        A game's score is based on it’s score from <a href="https://opencritic.com/" target="_blank">opencritic.com <font-awesome-icon icon="external-link-alt" size="sm" /></a>. Your team will gain or lose points based upon how well your game reviews.
        Only critic reviews are taken into account. For every point above a 70 on OpenCritic, your game gets you one point. For every point below 70,
        you lose one point. However, to really separate the cream of the crop, every point above 90 gets you <em>two</em> points. If a game doesn’t release in the
        year you are playing, it is treated as getting a 70, and therefore 0 points. Some examples (for the year 2018):
      </p>
      <table class="table table-striped table-bordered">
        <thead>
          <tr class="bg-primary">
            <th>Game Name</th>
            <th>Open Critic Score</th>
            <th>Fantasy Points</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td>Dragon Ball FighterZ</td>
            <td>86</td>
            <td>16</td>
          </tr>
          <tr>
            <td>Shadow of the Tomb Raider</td>
            <td>80</td>
            <td>10</td>
          </tr>
          <tr>
            <td>Fallout 76</td>
            <td>52</td>
            <td>-18</td>
          </tr>
          <tr>
            <td>Red Dead Redemption 2</td>
            <td>97</td>
            <td>34</td>
          </tr>
          <tr>
            <td>Kingdom Hearts III</td>
            <td>Delayed to 2019</td>
            <td>0</td>
          </tr>
        </tbody>
      </table>
      <h4>If I counter pick a game that scores below a 70, will I gain points?</h4>
      <p>
        Yes.
      </p>
      <h4>What is an “eligibility level”?</h4>
      <p>
        When deciding on the settings for a league, the league manager must choose what games to allow. The spirit of the game is to make predictions.
        In keeping with that spirit, some games are simply too safe of a bet, and don’t create interesting discussion. All games are categorized (by me) into “eligibility levels”,
        ranging from the most acceptable in all leagues, to games allowable in only the most lenient leagues.
      </p>
      <h4>What are the different levels?</h4>
      <ul>
        <li>
          <h5>New Game</h5>
          A definitively new game.
          <h6>Examples</h6>
          <ul>
            <li>Spider Man (2018)</li>
            <li>Red Dead Redemption 2</li>
            <li>Detroit: Become Human</li>
            <li>Call of Duty: Black Ops 4</li>
            <li>Assassin's Creed Odyssey</li>
          </ul>
        </li>
        <li>
          <h5>Complete Remake</h5>
          A remake that radically overhauls the original gameplay.
          <h6>Examples</h6>
          <ul>
            <li>Silent Hill: Shattered Memories</li>
            <li>Resident Evil 2 Remake</li>
            <li>Final Fantasy VII Remake</li>
            <li>Metroid: Samus Returns</li>
            <li>Tomb Raider: Anniversary</li>
          </ul>
        </li>
        <li>
          <h5>Remake</h5>
          A remake that modernizes gameplay without fundamentally changing it.
          <h6>Examples</h6>
          <ul>
            <li>Pokemon Omega Ruby and Alpha Sapphire</li>
            <li>Fire Emblem: Shadow Dragon</li>
            <li>Mario & Luigi: Superstar Saga + Bowser's Minions</li>
          </ul>
        </li>
        <li>
          <h5>Partial Remake</h5>
          A game that adds some new features while largely playing the same as the original.
          <h6>Examples</h6>
          <ul>
            <li>Crash Bandicoot N. Sane Trilogy</li>
            <li>The Legend of Zelda: Ocarina of Time 3D</li>
            <li>The Legend of Zelda: The Windwaker HD</li>
            <li>Metroid Prime Trilogy</li>
            <li>Shadow of the Colossus (2018)</li>
          </ul>
        </li>
        <li>
          <h5>Remaster</h5>
          A re-release that updates graphics while changing little else.
          <h6>Examples</h6>
          <ul>
            <li>Halo: The Master Chief Collection</li>
            <li>Uncharted: The Nathan Drake Collection</li>
            <li>Gears of War: Ultimate Edition</li>
            <li>Call of Duty: Modern Warfare Remastered</li>
          </ul>
        </li>
        <li>
          <h5>Port</h5>
          A game that was not originally released on a particular platform that is ported to it without many changes.
          <h6>Examples</h6>
          <ul>
            <li>Doom - Nintendo Switch Edition</li>
            <li>Sonic Mania Plus</li>
            <li>Captain Toad: Treasure Tracker (Nintendo Switch)</li>
          </ul>
        </li>
      </ul>
      <h4>
        I don't agree with your eligibility levels! My league is set to a maximum level of “Complete Remake” but I should be able
        to take the new 'Mario & Luigi: Bowser's Inside Story' on 3DS.
      </h4>
      <p>
        It's up to your league and ultimately up to your league manager. League managers can override the eligibility rules and assign
        the game you want to your team. Your league should agree on whether or not they want to allow the exception, though.
      </p>
      <h4>What do I get if I win?</h4>
      <p>
        That’s totally up to your league. This site doesn’t provide any prizes. Look to fantasy sports for advice on how to structure any winnings you want to play for.
      </p>
    </div>
  </div>
</template>
