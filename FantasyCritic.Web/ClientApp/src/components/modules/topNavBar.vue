<template>
    <div>
      <nav class="navbar navbar-expand bg-white main-nav">
        <router-link :to="{ name: 'welcome' }" class="navbar-brand">
          <template v-if="!isPlusUser">
            <img class="full-logo" src="/img/horizontal-logo.svg" />
            <img class="minimal-logo" src="/img/minimal-logo.svg" />
          </template>
          <template v-else>
            <img class="full-logo" src="/img/horizontal-logo-plus.svg" />
            <img class="minimal-logo" src="/img/minimal-logo-plus.svg" />
          </template>
        </router-link>
        <div class="navbar-collapse collapse">
          <ul class="navbar-nav">
            <li class="nav-item">
              <router-link :to="{ name: 'criticsRoyale', params: {year: activeRoyaleYear, quarter: activeRoyaleQuarter}}"
                           class="nav-link top-nav-link optional-nav critic-royale-nav-link" title="Critics Royale">
                <img class="critic-royale-nav minimal-nav topnav-icon" src="/img/critics-royale-top-nav.svg" />
                <span class="full-nav">Royale</span>
              </router-link>
            </li>
            <li class="nav-item">
              <router-link :to="{ name: 'howtoplay' }" class="nav-link top-nav-link super-optional-nav" title="How to Play">
                <font-awesome-icon class="minimal-nav topnav-icon" icon="book-open" size="lg" />
                <span class="full-nav">How to Play</span>
              </router-link>
            </li>
            <li class="nav-item" v-bind:class="{ 'optional-link': !isAuth }">
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
              <router-link :to="{ name: 'about' }" class="nav-link top-nav-link super-optional-nav" title="About">
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
            <li class="nav-item" v-bind:class="{ 'optional-link': !isAuth }">
              <router-link :to="{ name: 'fantasyCriticPlus' }" class="nav-link top-nav-link optional-nav" title="Fantasy Critic Plus">
                <img class="fc-plus-nav minimal-nav topnav-icon" src="/img/plus.svg" />
                <span class="full-nav">Plus</span>
              </router-link>
            </li>
          </ul>
        </div>
        <div class="my-2 my-lg-0" v-if="!authIsBusy">
          <ul class="navbar-nav">
            <li class="nav-item">
              <a class="nav-link top-nav-link brand-nav" href="https://patreon.com/fantasycritic" target="_blank">
                <font-awesome-icon :icon="['fab', 'patreon']" size="lg" class="topnav-icon patreon-icon" />
              </a>
            </li>
            <li class="nav-item">
              <a class="nav-link top-nav-link brand-nav" href="https://twitter.com/fantasy_critic" target="_blank">
                <font-awesome-icon :icon="['fab', 'twitter']" size="lg" class="topnav-icon twitter-icon" />
              </a>
            </li>
            <li class="nav-item">
              <a class="nav-link top-nav-link brand-nav" href="https://discord.gg/dNa7DD3" target="_blank">
                <font-awesome-icon :icon="['fab', 'discord']" size="lg" class="topnav-icon discord-icon" />
              </a>
            </li>
            <template v-if="isAuth && hasUserInfo">
              <li v-if="displayName" class="nav-item dropdown">
                <a class="nav-link dropdown-toggle" href="#" data-toggle="dropdown">
                  {{displayName}}
                  <span class="caret"></span>
                </a>
                <div class="dropdown-menu dropdown-menu-right top-nav-dropdown" aria-labelledby="navbarDropdown">
                  <a href="/Identity/Account/Manage" class="dropdown-item">Manage Account</a>
                  <div class="dropdown-divider"></div>
                  <a class="dropdown-item" href="/Identity/Account/Logout">Log off</a>
                </div>
              </li>
            </template>
            <template v-else>
              <li class="nav-item top-nav-button">
                <b-button variant="info" href="/Identity/Account/Login" class="nav-link">
                  <span>Log In</span>
                  <font-awesome-icon class="full-nav" icon="sign-in-alt" />
                </b-button>
              </li>
              <li class="nav-item">
                <b-button variant="primary" href="/Identity/Account/Register" class="nav-link">
                  <span>Sign Up</span>
                  <font-awesome-icon class="full-nav" icon="user-plus" />
                </b-button>
              </li>
            </template>
          </ul>
        </div>
      </nav>
    </div>
</template>

<script>
import axios from 'axios';

export default {
  computed: {
    isAuth() {
      return this.$store.getters.isAuthenticated;
    },
    hasUserInfo() {
      return this.$store.getters.userInfo;
    },
    displayName() {
      return this.$store.getters.userInfo.displayName;
    },
    authIsBusy() {
      return this.$store.getters.authIsBusy;
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
    },
    isPlusUser() {
      return this.$store.getters.isPlusUser;
    }
  },
  data() {
    return {
      activeRoyaleYearQuarter: null
    };
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
    },
    logout() {
      this.$store.dispatch('logout')
        .then(() => {
          this.$router.push({ name: 'login' });
        });
    }
  },
  async mounted() {
    await this.fetchActiveRoyaleYearQuarter();
  }
};
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
    height: 27px;
    padding-bottom: 3px;
  }

  .fc-plus-nav {
    height: 24px;
    padding-bottom: 3px;
    margin-right: 3px;
  }

  .plus-icon {
    display: inline;
  }

  @media only screen and (max-width: 1070px) {
    .full-logo {
      display: none;
    }
  }

  @media only screen and (min-width: 1071px) {
    .minimal-logo {
      display: none;
    }
  }

  @media only screen and (max-width: 370px) {
    .optional-nav {
      display: none;
    }
  }

  @media only screen and (max-width: 500px) {
    .super-optional-nav {
      display: none;
    }
  }

  @media only screen and (max-width: 435px) {
    .optional-link, .critic-royale-nav {
      display: none;
    }
  }

  @media only screen and (max-width: 680px) {
    .brand-nav {
      display: none;
    }
  }

  @media only screen and (max-width: 550px) {
    .topnav-icon {
      font-size: 15px;
    }

    .critic-royale-nav {
      height: 22px;
    }

    .fc-plus-nav {
      height: 19px;
    }
  }

  @media only screen and (max-width: 890px) {
    .full-nav {
      display: none;
    }
  }

  @media screen and (min-width: 889px) and (max-width: 1253px) {
    .minimal-nav {
      display: none;
    }
  }
</style>
