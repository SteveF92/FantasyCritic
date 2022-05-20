<template>
  <b-navbar toggleable="md" type="dark" variant="dark" class="main-nav">
    <b-navbar-brand :to="{ name: 'welcome' }">
      <template v-if="!isPlusUser">
        <img class="full-logo" src="/img/horizontal-logo.svg" />
        <img class="minimal-logo" src="/img/minimal-logo.svg" />
      </template>
      <template v-else>
        <img class="full-logo" src="/img/horizontal-logo-plus.svg" />
        <img class="minimal-logo" src="/img/minimal-logo-plus.svg" />
      </template>
    </b-navbar-brand>

    <b-navbar-toggle target="nav-collapse"></b-navbar-toggle>

    <b-collapse id="nav-collapse" is-nav>
      <b-navbar-nav>
        <b-nav-item :to="{ name: 'howtoplay' }">
          <font-awesome-icon class="topnav-icon" icon="book-open" size="lg" fixed-width />
          <span class="full-nav">How to Play</span>
        </b-nav-item>

        <b-nav-item :to="{ name: 'criticsRoyale' }">
          <img class="royale-icon topnav-icon" src="@/assets/critics-royale-top-nav.svg" />
          <span class="full-nav">Royale</span>
        </b-nav-item>

        <b-nav-item :to="{ name: 'masterGames' }">
          <font-awesome-icon class="topnav-icon" icon="gamepad" size="lg" fixed-width />
          <span class="full-nav">Games</span>
        </b-nav-item>

        <b-nav-item :to="{ name: 'faq' }">
          <font-awesome-icon class="topnav-icon" icon="question-circle" size="lg" fixed-width />
          <span class="full-nav">FAQ</span>
        </b-nav-item>

        <b-nav-item :to="{ name: 'about' }">
          <font-awesome-icon class="topnav-icon" icon="info-circle" size="lg" fixed-width />
          <span class="full-nav">About</span>
        </b-nav-item>

        <b-nav-item :to="{ name: 'community' }">
          <font-awesome-icon class="topnav-icon" icon="users" size="lg" fixed-width />
          <span class="full-nav">Community</span>
        </b-nav-item>

        <b-nav-item :to="{ name: 'contact' }">
          <font-awesome-icon class="topnav-icon" icon="envelope" size="lg" fixed-width />
          <span class="full-nav">Contact</span>
        </b-nav-item>

        <b-nav-item href="https://store.fantasycritic.games" target="_blank">
          <font-awesome-icon class="topnav-icon" icon="cart-shopping" size="lg" fixed-width />
          <span class="full-nav">Store</span>
        </b-nav-item>

        <b-nav-item :to="{ name: 'fantasyCriticPlus' }">
          <img class="fc-plus-icon topnav-icon" src="@/assets/plus.svg" />
          <span class="full-nav">Plus</span>
        </b-nav-item>
      </b-navbar-nav>

      <!-- Right aligned nav items -->
      <b-navbar-nav v-if="!authIsBusy" class="ml-auto user-dropdown">
        <b-nav-item href="https://patreon.com/fantasycritic" target="_blank">
          <font-awesome-icon :icon="['fab', 'patreon']" size="lg" class="patreon-icon" fixed-width />
          <span class="brand-text">Patreon</span>
        </b-nav-item>
        <b-nav-item href="https://twitter.com/fantasy_critic" target="_blank">
          <font-awesome-icon :icon="['fab', 'twitter']" size="lg" class="twitter-icon" fixed-width />
          <span class="brand-text">Twitter</span>
        </b-nav-item>
        <b-nav-item href="https://discord.gg/dNa7DD3" target="_blank">
          <font-awesome-icon :icon="['fab', 'discord']" size="lg" class="discord-icon" fixed-width />
          <span class="brand-text">Discord</span>
        </b-nav-item>

        <b-nav-item-dropdown v-if="isAuth && userInfo" right>
          <!-- Using 'button-content' slot -->
          <template #button-content>
            <span class="user-text">{{ displayName }}</span>
          </template>
          <b-dropdown-item href="/Account/Manage">Manage Account</b-dropdown-item>
          <b-dropdown-item href="/Account/Logout">Log Off</b-dropdown-item>
        </b-nav-item-dropdown>
        <template v-else>
          <b-button variant="info" href="/Account/Login">
            <span>Log In</span>
            <font-awesome-icon class="topnav-button-icon" icon="sign-in-alt" />
          </b-button>
          <b-button variant="primary" href="/Account/Register">
            <span>Sign Up</span>
            <font-awesome-icon class="topnav-button-icon" icon="user-plus" />
          </b-button>
        </template>
      </b-navbar-nav>
    </b-collapse>
  </b-navbar>
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

.full-logo,
.minimal-logo {
  height: 47px;
}

.topnav-icon,
.full-nav,
.brand-text,
.user-text {
  color: #d6993a;
}

.royale-icon {
  height: 27px;
  padding-bottom: 3px;
  margin-right: 1px;
}

.fc-plus-icon {
  height: 27px;
  padding-bottom: 3px;
  margin-right: 5px;
}

.nav-link {
  padding-left: 6px !important;
  padding-right: 6px !important;
}

.minimal-logo {
  display: none;
}

@media only screen and (max-width: 768px) {
  .user-dropdown {
    padding-bottom: 10px;
  }
}

@media only screen and (min-width: 768px) and (max-width: 1200px) {
  .full-logo {
    display: none;
  }

  .minimal-logo {
    display: block;
  }
}

@media only screen and (max-width: 1500px) {
  .topnav-button-icon {
    display: none;
  }
}

@media screen and (min-width: 1001px) and (max-width: 1460px) {
  .topnav-icon {
    display: none;
  }
}

@media only screen and (min-width: 768px) and (max-width: 1000px) {
  .full-nav {
    display: none;
  }
}

@media only screen and (min-width: 768px) {
  .brand-text {
    display: none;
  }
}
</style>
