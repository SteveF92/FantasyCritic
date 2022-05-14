<template>
  <div>
    <nav class="navbar navbar-expand bg-white main-nav">
      <router-link :to="{ name: 'welcome' }" class="navbar-brand">
        <template v-if="!isPlusUser">
          <img class="full-logo" src="@/assets/horizontal-logo.svg" />
          <img class="minimal-logo" src="@/assets/minimal-logo.svg" />
        </template>
        <template v-else>
          <img class="full-logo" src="@/assets/horizontal-logo-plus.svg" />
          <img class="minimal-logo" src="@/assets/minimal-logo-plus.svg" />
        </template>
      </router-link>
      <div class="navbar-collapse collapse">
        <ul class="navbar-nav">
          <li class="nav-item" :class="{ 'optional-link': !isAuth }">
            <router-link :to="{ name: 'criticsRoyale' }" class="nav-link" title="Critics Royale">
              <img class="topnav-image minimal-nav" src="@/assets/critics-royale-top-nav.svg" />
              <span class="full-nav">Royale</span>
            </router-link>
          </li>
          <li class="nav-item" :class="{ 'optional-link': isAuth }">
            <router-link :to="{ name: 'howtoplay' }" class="nav-link" title="How to Play">
              <font-awesome-icon class="topnav-icon minimal-nav" icon="book-open" size="lg" />
              <span class="full-nav">How to Play</span>
            </router-link>
          </li>
          <li class="nav-item">
            <router-link :to="{ name: 'faq' }" class="nav-link" title="FAQ">
              <font-awesome-icon class="topnav-icon minimal-nav" icon="question-circle" size="lg" />
              <span class="full-nav">FAQ</span>
            </router-link>
          </li>
          <li class="nav-item" :class="{ 'optional-link': !isAuth }">
            <router-link :to="{ name: 'masterGames' }" class="nav-link" title="Games">
              <font-awesome-icon class="topnav-icon minimal-nav" icon="gamepad" size="lg" />
              <span class="full-nav">Games</span>
            </router-link>
          </li>
          <li class="nav-item" :class="{ 'optional-link': isAuth }">
            <router-link :to="{ name: 'about' }" class="nav-link" title="About">
              <font-awesome-icon class="topnav-icon minimal-nav" icon="info-circle" size="lg" />
              <span class="full-nav">About</span>
            </router-link>
          </li>
          <li class="nav-item optional-link">
            <router-link :to="{ name: 'contact' }" class="nav-link" title="Contact">
              <font-awesome-icon class="topnav-icon minimal-nav" icon="envelope" size="lg" />
              <span class="full-nav">Contact</span>
            </router-link>
          </li>
          <li class="nav-item" :class="{ 'optional-link': !isAuth }">
            <a class="nav-link" href="https://store.fantasycritic.games" target="_blank">
              <font-awesome-icon class="topnav-icon minimal-nav" icon="cart-shopping" size="lg" />
              <span class="full-nav">Store</span>
            </a>
          </li>
          <li class="nav-item" :class="{ 'optional-link': !isAuth }">
            <router-link :to="{ name: 'fantasyCriticPlus' }" class="nav-link" title="Fantasy Critic Plus">
              <img class="topnav-image fc-plus-icon minimal-nav" src="@/assets/plus.svg" />
              <span class="full-nav">Plus</span>
            </router-link>
          </li>
        </ul>
      </div>
      <div v-if="!authIsBusy" class="my-2 my-lg-0">
        <ul class="navbar-nav">
          <li class="nav-item">
            <a class="nav-link brand-nav" href="https://patreon.com/fantasycritic" target="_blank">
              <font-awesome-icon :icon="['fab', 'patreon']" size="lg" class="topnav-icon patreon-icon" />
            </a>
          </li>
          <li class="nav-item">
            <a class="nav-link brand-nav" href="https://twitter.com/fantasy_critic" target="_blank">
              <font-awesome-icon :icon="['fab', 'twitter']" size="lg" class="topnav-icon twitter-icon" />
            </a>
          </li>
          <li class="nav-item">
            <a class="nav-link brand-nav" href="https://discord.gg/dNa7DD3" target="_blank">
              <font-awesome-icon :icon="['fab', 'discord']" size="lg" class="topnav-icon discord-icon" />
            </a>
          </li>
          <template v-if="isAuth && userInfo">
            <li v-if="displayName" class="nav-item dropdown">
              <a class="nav-link dropdown-toggle" href="#" data-toggle="dropdown">
                {{ displayName }}
                <span class="caret"></span>
              </a>
              <div class="dropdown-menu dropdown-menu-right top-nav-dropdown" aria-labelledby="navbarDropdown">
                <a href="/Account/Manage" class="dropdown-item">Manage Account</a>
                <div class="dropdown-divider"></div>
                <a class="dropdown-item" href="/Account/Logout">Log off</a>
              </div>
            </li>
          </template>
          <template v-else>
            <li class="nav-item top-nav-button">
              <b-button variant="info" href="/Account/Login" class="nav-link">
                <span>Log In</span>
                <font-awesome-icon class="topnav-button-icon" icon="sign-in-alt" />
              </b-button>
            </li>
            <li class="nav-item">
              <b-button variant="primary" href="/Account/Register" class="nav-link">
                <span>Sign Up</span>
                <font-awesome-icon class="topnav-button-icon" icon="user-plus" />
              </b-button>
            </li>
          </template>
        </ul>
      </div>
    </nav>
  </div>
</template>

<script>
import BasicMixin from '@/mixins/basicMixin';

export default {
  mixins: [BasicMixin]
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

.full-logo,
.minimal-logo {
  height: 47px;
}

.topnav-image {
  height: 27px;
  padding-bottom: 3px;
  margin-right: 3px;
}

@media only screen and (max-width: 1115px) {
  .full-logo {
    display: none;
  }
}

@media only screen and (min-width: 1116px) {
  .minimal-logo {
    display: none;
  }
}

@media only screen and (max-width: 935px) {
  .full-nav {
    display: none;
  }
}

@media screen and (min-width: 936px) and (max-width: 1320px) {
  .minimal-nav {
    display: none;
  }
}

@media only screen and (max-width: 765px) {
  .optional-link {
    display: none;
  }
}

@media only screen and (max-width: 490px) {
  .brand-nav {
    display: none;
  }
}

@media only screen and (max-width: 790px) {
  .topnav-button-icon {
    display: none;
  }
}
</style>
