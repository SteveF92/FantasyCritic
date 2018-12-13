<template>
    <div>
        <nav class="navbar navbar-expand-lg navbar-dark bg-dark" id="top-nav">
            <router-link :to="{ name: 'welcome' }" class="navbar-brand" title="Fantasy Critic">Fantasy Critic</router-link>
            <button class="navbar-toggler collapsed" type="button" data-toggle="collapse" data-target="#navbar-collapsible" aria-controls="navbar-collapsible" aria-expanded="false" aria-label="Toggle navigation" style="">
                <span class="navbar-toggler-icon"></span>
            </button>

            <div class="navbar-collapse collapse" id="navbar-collapsible">
                <ul class="navbar-nav mr-auto">
                    <li class="nav-item">
                        <router-link :to="{ name: 'about' }" class="nav-link" title="About">About</router-link>
                    </li>
                    <li class="nav-item">
                        <router-link :to="{ name: 'contact' }" class="nav-link" title="Contact">Contact</router-link>
                    </li>
                </ul>
                
                <div class="my-2 my-lg-0">
                  <ul class="navbar-nav mr-auto">
                    <li class="nav-item">
                      <a class="nav-link nav-icon" href="https://twitter.com/fantasy_critic" target="_blank">
                        <icon :icon="['fab', 'twitter-square']" />
                      </a>
                    </li>
                    <li class="nav-item">
                      <a class="nav-link nav-icon" href="https://www.reddit.com/r/fantasycritic/" target="_blank">
                        <icon :icon="['fab', 'reddit-square']" />
                      </a>
                    </li>
                    <slot v-if="isAuth">
                      <li class="nav-item">
                        <a class="nav-link nav-icon" href="#">
                          <icon icon="bell" />
                        </a>
                      </li>
                      <li v-if="userName" class="nav-item dropdown">
                        <a class="nav-link dropdown-toggle" href="#" data-toggle="dropdown">
                          {{userName}}
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
                        <b-button variant="primary" :to="{ name: 'login' }" class="nav-link">Log in</b-button>
                      </li>
                      <li class="nav-item">
                        <b-button variant="info" :to="{ name: 'register' }" class="nav-link">Sign Up</b-button>
                      </li>
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
                return this.$store.getters.tokenIsCurrent(new Date());
            },
            userName() {
                return this.$store.getters.userInfo.userName;
            },
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

.nav-icon {
    font-size: 1.1rem;
}

#top-nav {
    max-height: 50px;
    padding-bottom: 0;
}

#navbar-collapsible {
    z-index: 10;
    background-color: #4E5D6C;
}

</style>
