<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <h1>Community</h1>
    <hr />
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
    <h2>Hype Factor</h2>
    <div class="text-well hype-factor-section">
      <img src="@/assets/hype-factor-logo.png" />
      <div>
        <p>
          Hype Factor is a monthly Fantasy Critic podcast hosted by
          <a
            href="https://bsky.app/profile/jon32.bsky.social
">
            Jon32
          </a>
          and
          <a href="https://bsky.app/profile/marcusoneill79.bsky.social">Marcus O'Neill</a>
          . In each episode, they breakdown the year's releases, discuss the news and predict review scores - all through a Fantasy Critic lens.
        </p>
        <p>
          While Hype Factor is not an "official" Fantasy Critic podcast and I, Steve Fallon, have no editorial control over it, I can confidently say that these two have a great grasp on the gaming
          world and Fantasy Critic strategy. If you are looking for a place to start when looking to get more engaged with Fantasy Critic throughout the year, this is where I would start.
        </p>

        <p>
          Hype Factor can be found on most podcast platforms, including
          <a href="https://open.spotify.com/show/1hMMij30x9CNbToeHm2q06">Spotify</a>
          and
          <a href="https://podcasts.apple.com/us/podcast/hype-factor-a-fantasy-critic-podcast/id1784246680">Apple Podcasts</a>
          .
        </p>
      </div>
    </div>
    <h2>Your creations!</h2>
    <div class="text-well">Have you made something Fantasy Critic related that you think deserves a spot here? Send me a message and maybe I'll add it.</div>
  </div>
</template>
<style scoped>
.hype-factor-section {
  display: flex;
  align-items: flex-start;
  gap: 10px;
}
</style>
