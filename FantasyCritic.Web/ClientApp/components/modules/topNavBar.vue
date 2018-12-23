<template>
    <div>
      <nav class="navbar navbar-expand bg-white main-nav">
        <router-link :to="{ name: 'welcome' }" class="navbar-brand">
          <img class="full-logo" src="/images/horizontal-logo.png" />
          <img class="minimal-logo" src="/images/minimal-logo.png" />
        </router-link>
        <div class="navbar-collapse collapse">
          <ul class="navbar-nav">
            <li class="nav-item">
              <router-link :to="{ name: 'about' }" class="nav-link top-nav-link optional-nav" title="About">
                <span class="full-nav">About</span>
                <font-awesome-icon class="minimal-nav topnav-icon" icon="info-circle" size="lg" />
              </router-link>
            </li>
            <li class="nav-item">
              <router-link :to="{ name: 'faq' }" class="nav-link top-nav-link optional-nav" title="FAQ">
                <span class="full-nav">FAQ</span>
                <font-awesome-icon class="minimal-nav topnav-icon" icon="question-circle" size="lg" />
              </router-link>
            </li>
            <li class="nav-item">
              <router-link :to="{ name: 'contact' }" class="nav-link top-nav-link optional-nav" title="Contact">
                <span class="full-nav">Contact</span>
                <font-awesome-icon class="minimal-nav topnav-icon" icon="envelope" size="lg" />
              </router-link>
            </li>
          </ul>
        </div>
        <div class="my-2 my-lg-0">
          <ul class="navbar-nav">
            <li class="nav-item">
              <a class="nav-link top-nav-link brand-nav" href="https://twitter.com/fantasy_critic" target="_blank">
                <font-awesome-icon :icon="['fab', 'twitter-square']" size="lg" :style="{ color: '00acee' }" class="topnav-icon" />
              </a>
            </li>
            <li class="nav-item">
              <a class="nav-link top-nav-link brand-nav" href="https://www.reddit.com/r/fantasycritic/" target="_blank">
                <font-awesome-icon :icon="['fab', 'reddit-square']" size="lg" :style="{ color: 'ff4500' }" class="topnav-icon" />
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
          }
        },
        methods: {
            logout() {
                this.$store.dispatch("logout")
                    .then(() => {
                        this.$router.push({ name: "login" });
                    });
            }
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

  @media only screen and (max-width: 679px) {
    .full-logo {
      display: none;
    }
  }

  @media only screen and (min-width: 680px) {
    .minimal-logo {
      display: none;
    }
  }

  @media only screen and (max-width: 340px) {
    .optional-nav {
      display: none;
    }
  }

  @media only screen and (max-width: 430px) {
    .topnav-icon {
      font-size: 15px;
    }
    .brand-nav {
      display: none;
    }
  }

  @media only screen and (max-width: 509px) {
    .full-nav {
      display: none;
    }
  }

  @media only screen and (min-width: 510px) {
    .minimal-nav {
      display: none;
    }
  }
</style>
