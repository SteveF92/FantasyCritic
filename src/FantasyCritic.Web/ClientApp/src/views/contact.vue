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

      <h2>Community</h2>
      <div class="text-well">
        <h3>
          <a href="https://discord.gg/dNa7DD3" target="_blank">
            <font-awesome-icon :icon="['fab', 'discord']" size="lg" class="discord-icon" />
            Join our Discord Server!
          </a>
        </h3>
        <div>
          <p>The Discord is the primary home for the Fantasy Critic community. We have channels for:</p>
          <ul>
            <li>Finding a league</li>
            <li>Rating each other's game lineups</li>
            <li>Debating what tags a game should have</li>
            <li>Tech Support</li>
            <li>Discussing new features</li>
            <li>...and more</li>
          </ul>
        </div>
        <h2>
          <a href="https://www.reddit.com/r/fantasycritic/" target="_blank">
            <font-awesome-icon :icon="['fab', 'reddit']" size="lg" class="reddit-icon" />
            r/fantasycritic
          </a>
        </h2>
        <div>
          <p>
            The subreddit admittedly doesn't see as much traffic as it used to, as it's been largely supplanted by our Discord. However, it may still be where you want to go if you want to post
            something very detailed like a feature proposal. I do still check it, but you'll get a faster response from both myself and the community on our Discord.
          </p>
        </div>
      </div>

      <h2>How to Reach Me Directly</h2>
      <div class="text-well">
        <ul class="unbulleted-list">
          <li>
            <a href="https://twitter.com/fantasy_critic" target="_blank">
              <font-awesome-icon :icon="['fab', 'twitter']" size="lg" class="twitter-icon" fixed-width />
              @fantasy_critic
            </a>
          </li>
          <li>
            <font-awesome-icon :icon="['fab', 'discord']" size="lg" class="discord-icon" fixed-width />
            elitefusion#8608 (You can find me at the top of the members list in our Discord)
          </li>
        </ul>
        <h3>Account issues</h3>
        <p>
          If you have an issue with your account(can't login, can't verify email, etc), please reach out to me directly via DM on Discord or Twitter. Additionally, if one of your league members is
          having issues with their account, you should have them reach out to me directly, rather than contacting me on their behalf.
        </p>
        <h3>Other Issues</h3>
        <p>
          I'm always happy to respond to a DM, but if your issue is not an account issue, you can also post in our #tech-support channel in our Discord. In particular, if you have an issue with the
          <router-link :to="{ name: 'discordBot' }">Discord Bot</router-link>
          , you should definitely post that in the channel as opposed to a DM.
        </p>
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
