<template>
  <div id="vue-app">
    <div id="top-nav-bar">
      <top-navbar></top-navbar>
    </div>
    <div class="container-fluid">
      <div class="row">
        <main class="col-12">
          <router-view></router-view>
        </main>
      </div>
    </div>
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
              <router-link :to="{ name: 'about' }" title="About">
                <font-awesome-icon icon="info-circle" size="lg" />
                About
              </router-link>
            </div>
            <div>
              <router-link :to="{ name: 'faq' }" title="FAQ">
                <font-awesome-icon icon="question-circle" size="lg" />
                FAQ
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
          </div>
        </div>
      </div>
    </footer>
  </div>
</template>

<script>
  import TopNavbar from "components/modules/topNavbar";

  export default {
    components: {
      TopNavbar
    },
    mounted() {
      if (this.$store.getters.tokenIsCurrent()) {
        this.$store.dispatch("getUserInfo");
      }
    }
  };
</script>

<style>
  [v-cloak] {
    display: none;
  }

  html {
    position: relative;
    min-height: 100%;
  }
</style>

<style scoped>
  @media only screen and (max-width: 343px) {
    .full-contact {
      display: none;
    }
  }

  @media only screen and (max-width: 679px) {
    .full-logo {
      display: none;
    }

    #vue-app {
      margin-bottom: 75px; /* Margin bottom by footer height */
    }

    .footer {
      height: 75px; /* Set the fixed height of the footer here */
    }

    .container {
      padding-bottom: 25px;
    }
  }

  @media only screen and (min-width: 680px) {
    .small-logo {
      display: none;
    }

    #vue-app {
      margin-bottom: 175px; /* Margin bottom by footer height */
    }

    .footer {
      height: 175px; /* Set the fixed height of the footer here */
    }
  }

  .footer {
    position: absolute;
    background-color: black;
    bottom: 0;
    width: 100%;
    line-height: 30px; /* Vertically center the text there */
  }

  /* Custom page CSS
-------------------------------------------------- */
  /* Not required for template or sticky footer method. */

  .container {
    width: auto;
    max-width: 1200px;
    padding-left: 45px;
    padding-right: 45px;
    background-color: black;
  }

</style>

<style lang="scss">
  @import '../scss/custom-bootstrap.scss';
  @import '../../node_modules/bootstrap/scss/bootstrap.scss';
</style>

<style>
@import '../css/site.css';
@import '../css/modal.css';
</style>
