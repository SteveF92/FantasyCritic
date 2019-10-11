<template>
    <div>
      <nav class="navbar navbar-expand bg-white main-nav">
        <router-link :to="{ name: 'welcome' }" class="navbar-brand">
          <img class="full-logo" src="/images/horizontal-logo.svg" />
          <img class="minimal-logo" src="/images/minimal-logo.svg" />
        </router-link>
        <div class="navbar-collapse collapse">
          <ul class="navbar-nav">
            <li class="nav-item">
              <router-link :to="{ name: 'howtoplay' }" class="nav-link top-nav-link optional-nav" title="How to Play">
                <font-awesome-icon class="minimal-nav topnav-icon" icon="book-open" size="lg" />
                <span class="full-nav">How to Play</span>
              </router-link>
            </li>
            <li class="nav-item">
            <router-link :to="{ name: 'criticsRoyale', params: {year: activeRoyaleYear, quarter: activeRoyaleQuarter}}"
                         class="nav-link top-nav-link optional-nav" title="Critics Royale">
              <img class="critic-royale-nav" src="/images/critics-royale-top-nav.svg" />
              <span class="full-nav">Royale</span>
            </router-link>
            </li>
            <li class="nav-item">
              <router-link :to="{ name: 'faq' }" class="nav-link top-nav-link optional-nav" title="FAQ">
                <font-awesome-icon class="minimal-nav topnav-icon" icon="question-circle" size="lg" />
                <span class="full-nav">FAQ</span>
              </router-link>
            </li>
            <li class="nav-item" v-bind:class="{ 'optional-link': !isAuth }">
              <router-link :to="{ name: 'masterGames' }" class="nav-link top-nav-link optional-nav" title="Games">
                <font-awesome-icon class="minimal-nav topnav-icon" icon="gamepad" size="lg" />
                <span class="full-nav">Games</span>
              </router-link>
            </li>
            <li class="nav-item">
              <router-link :to="{ name: 'about' }" class="nav-link top-nav-link optional-nav" title="About">
                <font-awesome-icon class="minimal-nav topnav-icon" icon="info-circle" size="lg" />
                <span class="full-nav">About</span>
              </router-link>
            </li>
            <li class="nav-item" v-bind:class="{ 'optional-link': !isAuth }">
              <router-link :to="{ name: 'contact' }" class="nav-link top-nav-link optional-nav" title="Contact">
                <font-awesome-icon class="minimal-nav topnav-icon" icon="envelope" size="lg" />
                <span class="full-nav">Contact</span>
              </router-link>
            </li>
          </ul>
        </div>
        <div class="my-2 my-lg-0">
          <ul class="navbar-nav">
            <li class="nav-item">
              <a class="nav-link top-nav-link brand-nav" href="https://www.facebook.com/fantasycritic" target="_blank">
                <font-awesome-icon :icon="['fab', 'facebook-square']" size="lg" class="topnav-icon fb-icon" />
              </a>
            </li>
            <li class="nav-item">
              <a class="nav-link top-nav-link brand-nav" href="https://twitter.com/fantasy_critic" target="_blank">
                <font-awesome-icon :icon="['fab', 'twitter-square']" size="lg" class="topnav-icon twitter-icon" />
              </a>
            </li>
            <li class="nav-item">
              <a class="nav-link top-nav-link brand-nav" href="https://www.reddit.com/r/fantasycritic/" target="_blank">
                <font-awesome-icon :icon="['fab', 'reddit-square']" size="lg" class="topnav-icon reddit-icon" />
              </a>
            </li>
            <slot v-if="!storeIsBusy">
              <slot v-if="isAuth && hasUserInfo">
                <li v-if="displayName" class="nav-item dropdown">
                  <a class="nav-link dropdown-toggle" href="#" data-toggle="dropdown">
                    {{displayName}}
                    <span class="caret"></span>
                  </a>
                  <div class="dropdown-menu dropdown-menu-right top-nav-dropdown" aria-labelledby="navbarDropdown">
                    <router-link :to="{ name: 'manageUser' }" class="dropdown-item">Manage Account</router-link>
                    <div class="dropdown-divider"></div>
                    <a class="dropdown-item" href="#" v-on:click="logout()">Log off</a>
                  </div>
                </li>
              </slot>
              <slot v-else>
                <li class="nav-item top-nav-button">
                  <b-button variant="info" :to="{ name: 'login' }" class="nav-link">
                    <span>Log In</span>
                    <font-awesome-icon class="full-nav" icon="sign-in-alt" />
                  </b-button>
                </li>
                <li class="nav-item">
                  <b-button variant="primary" :to="{ name: 'register' }" class="nav-link">
                    <span>Sign Up</span>
                    <font-awesome-icon class="full-nav" icon="user-plus" />
                  </b-button>
                </li>
              </slot>
            </slot>
          </ul>
        </div>
      </nav>
    </div>
</template>

<script>
  import axios from "axios";

  export default {
    computed: {
      isAuth() {
          return this.$store.getters.tokenIsCurrent();
      },
      hasUserInfo() {
        return this.$store.getters.userInfo;
      },
      displayName() {
        return this.$store.getters.userInfo.displayName;
      },
      storeIsBusy() {
        return this.$store.getters.storeIsBusy;
      },
      activeRoyaleYear() {
        if (!this.activeRoyaleYearQuarter) {
          return;
        }
        return this.activeRoyaleYearQuarter.year;
      },
      activeRoyaleQuarter() {
        if (!this.activeRoyaleYearQuarter) {
          return;
        }
        return this.activeRoyaleYearQuarter.quarter;
      }
    },
    methods: {
      logout() {
        this.$store.dispatch("logout")
          .then(() => {
              this.$router.push({ name: "login" });
          });
      }
    },
    data() {
      return {
        activeRoyaleYearQuarter: null
      }
    },
    methods: {
      async fetchActiveRoyaleYearQuarter() {
        axios
          .get('/api/royale/ActiveRoyaleQuarter')
          .then(response => {
            this.activeRoyaleYearQuarter = response.data;
          })
          .catch(response => {

          });
      }
    },
    async mounted() {
      await this.fetchActiveRoyaleYearQuarter();
    }
  }
</script>

<style scoped>
  .main-nav {
    padding-top: 0;
    padding-bottom: 0;
  }

  .navbar-brand {
    margin-right: 3px;
  }

  .top-nav-button {
    margin-right: 5px;
  }

  .full-logo, .minimal-logo {
    height: 47px;
  }

  .critic-royale-nav{
    height: 23px;
  }

  @media only screen and (max-width: 940px) {
    .full-logo {
      display: none;
    }
  }

  @media only screen and (min-width: 941px) {
    .minimal-logo {
      display: none;
    }
  }

  @media only screen and (max-width: 350px) {
    .optional-nav {
      display: none;
    }
  }

  @media only screen and (max-width: 400px) {
    .optional-link {
      display: none;
    }
  }

  @media only screen and (max-width: 520px) {
    .brand-nav {
      display: none;
    }
  }

  @media only screen and (max-width: 425px) {
    .topnav-icon {
      font-size: 15px;
    }
  }

  @media only screen and (max-width: 730px) {
    .full-nav {
      display: none;
    }
  }

  @media screen and (min-width: 729px) and (max-width: 1100px) {
    .minimal-nav {
      display: none;
    }
  }
</style>
