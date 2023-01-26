<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <h1>Contact</h1>
      <hr />
      <h2>Adding new Games/Corrections</h2>
      <div class="text-well">
        <ul>
          <li>
            Please use the
            <router-link :to="{ name: 'masterGameRequest' }">Master Game Request form</router-link>
            for adding new games.
          </li>
          <li>Please use the "Suggest Correction" form, which can be accessed by clicking on a master game link and clicking the "Suggest a Correction" option for correcting existing games.</li>
          <li>
            If you have a particularly "contentious" question, like debating what tags a game should have, please bring those questions to #eligibility-debates in our
            <a href="https://discord.gg/dNa7DD3">
              Discord
              <font-awesome-icon icon="external-link-alt" size="sm" />
            </a>
            .
          </li>
        </ul>
      </div>
      <div>
        <h2>Twitter</h2>
        <div class="text-well">
          <p>You can reach me on Twitter for any questions, comments, or support.</p>
          <a href="https://twitter.com/fantasy_critic" target="_blank">
            <font-awesome-icon :icon="['fab', 'twitter']" size="lg" class="twitter-icon" fixed-width />
            @fantasy_critic
          </a>
        </div>
      </div>
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
        <h2>Your creations!</h2>
        <div class="text-well">Have you made something Fantasy Critic related that you think is cool? Contact me and I might just feature it here.</div>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  data() {
    return {};
  }
};
</script>
