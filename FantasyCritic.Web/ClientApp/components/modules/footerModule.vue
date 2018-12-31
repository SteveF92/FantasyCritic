<template>
  <footer class="footer">
    <div class="container">
      <div class="row justify-content-between">
        <div col-md-4>
          <img class="full-logo" src="/images/logo-white.png" />
          <img class="small-logo" src="/images/small-logo-white.png" />
        </div>
        <div col-md-4>
          <label>Navigation</label>
          <div>
            <router-link :to="{ name: 'howtoplay' }" title="How To Play">
              <font-awesome-icon icon="book-open" size="lg" />
              How to Play
            </router-link>
          </div>
          <div>
            <router-link :to="{ name: 'faq' }" title="FAQ">
              <font-awesome-icon icon="question-circle" size="lg" />
              FAQ
            </router-link>
          </div>
          <div>
            <router-link :to="{ name: 'about' }" title="About">
              <font-awesome-icon icon="info-circle" size="lg" />
              About
            </router-link>
          </div>
        </div>
        <div col-md-4>
          <label>Contact</label>
          <div>
            <a href="https://twitter.com/fantasy_critic" target="_blank">
              <font-awesome-icon :icon="['fab', 'twitter-square']" size="lg" :style="{ color: '00acee' }" />
              <span class="full-contact">@fantasy_critic</span>
            </a>
          </div>
          <div>
            <a href="https://www.reddit.com/r/fantasycritic/" target="_blank">
              <font-awesome-icon :icon="['fab', 'reddit-square']" size="lg" :style="{ color: 'ff4500' }" />
              <span class="full-contact">r/fantasycritic</span>
            </a>
          </div>
          <div>
            <a href="https://www.facebook.com/fantasycritic" target="_blank">
              <font-awesome-icon :icon="['fab', 'facebook-square']" size="lg" :style="{ color: '3b5998' }" />
              <span class="full-contact">Facebook</span>
            </a>
          </div>
          <div>
            <a href="https://github.com/SteveF92/FantasyCritic" target="_blank">
              <font-awesome-icon :icon="['fab', 'github-square']" size="lg" :style="{ color: 'ffffff' }" />
              <span class="full-contact">GitHub</span>
            </a>
          </div>
        </div>
      </div>
    </div>
  </footer>
</template>
