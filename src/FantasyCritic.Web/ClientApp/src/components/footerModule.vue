<template>
  <footer class="footer">
    <div class="container">
      <div class="row justify-content-between">
        <div class="footer-logo" col-lg-4>
          <img src="@/assets/logo-white.png" />
        </div>
        <div col-lg-4 col-md-6>
          <div>
            <router-link :to="{ name: 'howtoplay' }" title="How To Play">
              <font-awesome-icon icon="book-open" size="lg" fixed-width />
              <span class="full-nav">How to Play</span>
            </router-link>
          </div>
          <div>
            <router-link :to="{ name: 'masterGames' }" title="Games">
              <font-awesome-icon icon="gamepad" size="lg" fixed-width />
              <span class="full-nav">Games</span>
            </router-link>
          </div>
          <div>
            <router-link :to="{ name: 'faq' }" title="FAQ">
              <font-awesome-icon icon="question-circle" size="lg" fixed-width />
              <span class="full-nav">FAQ</span>
            </router-link>
          </div>
          <div>
            <router-link :to="{ name: 'about' }" title="About">
              <font-awesome-icon icon="info-circle" size="lg" fixed-width />
              <span class="full-nav">About</span>
            </router-link>
          </div>
          <div>
            <router-link :to="{ name: 'community' }" title="Community">
              <font-awesome-icon icon="users" size="lg" fixed-width />
              <span class="full-nav">Community</span>
            </router-link>
          </div>
          <div>
            <a href="https://store.fantasycritic.games" target="_blank">
              <font-awesome-icon icon="cart-shopping" size="lg" fixed-width />
              <span class="full-nav">Store</span>
            </a>
          </div>
          <div>
            <router-link :to="{ name: 'fantasyCriticPlus' }" title="Fantasy Critic Plus">
              <img class="fc-plus-nav" src="@/assets/plus.svg" />
              <span class="full-nav">Fantasy Critic Plus</span>
            </router-link>
          </div>
        </div>
        <div col-lg-4 col-md-6>
          <div>
            <a href="https://patreon.com/fantasycritic" target="_blank">
              <font-awesome-icon :icon="['fab', 'patreon']" size="lg" class="patreon-icon" fixed-width />
              <span class="full-contact">Patreon</span>
            </a>
          </div>
          <div>
            <a href="https://twitter.com/fantasy_critic" target="_blank">
              <font-awesome-icon :icon="['fab', 'twitter']" size="lg" class="twitter-icon" fixed-width />
              <span class="full-contact">@fantasy_critic</span>
            </a>
          </div>
          <div>
            <a href="https://www.reddit.com/r/fantasycritic/" target="_blank">
              <font-awesome-icon :icon="['fab', 'reddit']" size="lg" class="reddit-icon" fixed-width />
              <span class="full-contact">r/fantasycritic</span>
            </a>
          </div>
          <div>
            <a href="https://discord.gg/dNa7DD3" target="_blank">
              <font-awesome-icon :icon="['fab', 'discord']" size="lg" class="discord-icon" fixed-width />
              <span class="full-contact">Discord</span>
            </a>
          </div>
          <div>
            <a href="https://www.facebook.com/fantasycritic" target="_blank">
              <font-awesome-icon :icon="['fab', 'facebook']" size="lg" class="fb-icon" fixed-width />
              <span class="full-contact">Facebook</span>
            </a>
          </div>
          <div>
            <a href="https://github.com/SteveF92/FantasyCritic" target="_blank">
              <font-awesome-icon :icon="['fab', 'github']" size="lg" class="github-icon" fixed-width />
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
  padding-left: 2px;
  margin-right: 5px;
}
</style>
