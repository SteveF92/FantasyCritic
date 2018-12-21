<template>
    <div>
      <nav class="navbar navbar-expand-lg" id="top-nav">
        <router-link :to="{ name: 'welcome' }" class="navbar-brand">
          <img class="main-logo" src="/images/horizontal-logo.png" />
        </router-link>
        <button class="navbar-toggler collapsed" type="button" data-toggle="collapse" data-target="#navbar-collapsible" aria-controls="navbar-collapsible" aria-expanded="false" aria-label="Toggle navigation" style="">
          <span class="navbar-toggler-icon"></span>
        </button>

        <div class="navbar-collapse collapse" id="navbar-collapsible">
          <ul class="navbar-nav mr-auto">
            <li class="nav-item">
              <router-link :to="{ name: 'about' }" class="nav-link top-nav-link" title="About">About</router-link>
            </li>
            <li class="nav-item">
              <router-link :to="{ name: 'faq' }" class="nav-link top-nav-link" title="FAQ">FAQ</router-link>
            </li>
            <li class="nav-item">
              <router-link :to="{ name: 'contact' }" class="nav-link top-nav-link" title="Contact">Contact</router-link>
            </li>
          </ul>

          <div class="my-2 my-lg-0">
            <ul class="navbar-nav mr-auto">
              <li class="nav-item top-nav-brand-icon">
                <a class="nav-icon" href="https://twitter.com/fantasy_critic" target="_blank">
                  <font-awesome-icon :icon="['fab', 'twitter-square']" size="lg" />
                </a>
              </li>
              <li class="nav-item top-nav-brand-icon">
                <a class="nav-icon" href="https://www.reddit.com/r/fantasycritic/" target="_blank">
                  <font-awesome-icon :icon="['fab', 'reddit-square']" size="lg" />
                </a>
              </li>
              <slot v-if="!storeIsBusy">
                <slot v-if="isAuth && hasUserInfo">
                  <!--<li class="nav-item">
              <a class="nav-link nav-icon" href="#">
                <font-awesome-icon icon="bell" />
              </a> 
            </li>-->
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
                  <li class="nav-item">
                    <b-button variant="info" :to="{ name: 'login' }" class="nav-link">Log in</b-button>
                  </li>
                  <li class="nav-item">
                    <b-button variant="primary" :to="{ name: 'register' }" class="nav-link">Sign Up</b-button>
                  </li>
                </slot>
              </slot>
            </ul>
          </div>
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
.navbar-nav .nav-item {
    margin-left: 5px;
    margin-right: 5px;
}

.main-logo{
  padding-bottom: 10px;
}

#top-nav {
  max-height: 50px;
  padding-bottom: 0;
  background-color: #ffffff;
}

#navbar-collapsible {
    z-index: 10;
}

a.top-nav-link {
  color: black !important;
  font-weight: bold;
  text-transform: uppercase;
}

.top-nav-brand-icon {
  margin-top: 6px;
}

</style>
