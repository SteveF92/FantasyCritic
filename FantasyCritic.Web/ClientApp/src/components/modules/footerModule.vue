<template>
  <footer class="footer">
    <div class="container">
      <div class="row justify-content-between">
        <div col-md-4>
          <img class="full-logo" src="/img/logo-white.png" />
          <img class="small-logo" src="/img/small-logo-white.png" />
        </div>
        <div col-md-4>
          <label>Navigation</label>
          <div>
            <router-link :to="{ name: 'howtoplay' }" title="How To Play">
              <font-awesome-icon icon="book-open" size="lg" />
              <span class="full-nav">How to Play</span>
            </router-link>
          </div>
          <div>
            <router-link :to="{ name: 'masterGames' }" title="Games">
              <font-awesome-icon icon="gamepad" size="lg" />
              <span class="full-nav">Games</span>
            </router-link>
          </div>
          <div>
            <router-link :to="{ name: 'faq' }" title="FAQ">
              <font-awesome-icon icon="question-circle" size="lg" />
              <span class="full-nav">FAQ</span>
            </router-link>
          </div>
          <div>
            <router-link :to="{ name: 'about' }" title="About">
              <font-awesome-icon icon="info-circle" size="lg" />
              <span class="full-nav">About</span>
            </router-link>
          </div>
          <div>
            <router-link :to="{ name: 'fantasyCriticPlus' }" title="Fantasy Critic Plus">
              <img class="fc-plus-nav" src="/img/plus.svg" />
              <span class="full-nav">Fantasy Critic Plus</span>
            </router-link>
          </div>
        </div>
        <div col-md-4>
          <div>
            <a href="https://patreon.com/fantasycritic" target="_blank">
              <font-awesome-icon :icon="['fab', 'patreon']" size="lg" class="patreon-icon" />
              <span class="full-contact">Patreon</span>
            </a>
          </div>
          <div>
            <a href="https://twitter.com/fantasy_critic" target="_blank">
              <font-awesome-icon :icon="['fab', 'twitter-square']" size="lg" class="twitter-icon" />
              <span class="full-contact">@fantasy_critic</span>
            </a>
          </div>
          <div>
            <a href="https://www.reddit.com/r/fantasycritic/" target="_blank">
              <font-awesome-icon :icon="['fab', 'reddit-square']" size="lg" class="reddit-icon" />
              <span class="full-contact">r/fantasycritic</span>
            </a>
          </div>
          <div>
            <a href="https://discord.gg/dNa7DD3" target="_blank">
              <font-awesome-icon :icon="['fab', 'discord']" size="lg" class="discord-icon" />
              <span class="full-contact">Discord</span>
            </a>
          </div>
          <div>
            <a href="https://www.facebook.com/fantasycritic" target="_blank">
              <font-awesome-icon :icon="['fab', 'facebook-square']" size="lg" class="fb-icon" />
              <span class="full-contact">Facebook</span>
            </a>
          </div>
          <div>
            <a href="https://github.com/SteveF92/FantasyCritic" target="_blank">
              <font-awesome-icon :icon="['fab', 'github-square']" size="lg" class="github-icon" />
              <span class="full-contact">GitHub</span>
            </a>
          </div>
        </div>
      </div>
    </div>
  </footer>
</template>
<style scoped>
  .fc-plus-nav {
    height: 24px;
    padding-bottom: 3px;
    margin-right: 3px;
  }
</style>
