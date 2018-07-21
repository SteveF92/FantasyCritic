<template>
    <div>
        <nav class="navbar navbar-expand-lg navbar-dark bg-dark">
            <router-link :to="{ name: 'home' }" class="navbar-brand" title="Fantasy Critic">Fantasy Critic</router-link>
            <button class="navbar-toggler collapsed" type="button" data-toggle="collapse" data-target="#navbarColor02" aria-controls="navbarColor02" aria-expanded="false" aria-label="Toggle navigation" style="">
                <span class="navbar-toggler-icon"></span>
            </button>

            <div class="navbar-collapse collapse" id="navbarColor02" style="">
                <ul class="navbar-nav mr-auto">
                    <li class="nav-item">
                        <router-link :to="{ name: 'about' }" class="nav-link" title="About">About</router-link>
                    </li>
                    <li class="nav-item">
                        <router-link :to="{ name: 'contact' }" class="nav-link" title="Contact">Contact</router-link>
                    </li>
                </ul>
                <div v-cloak v-if="credsLoaded">
                    <div v-if="isAuth">
                        <div class="my-2 my-lg-0">
                            <ul class="navbar-nav mr-auto">
                                <li class="nav-item">
                                    <form class="form-inline my-2 my-lg-0">
                                        <input class="form-control mr-sm-2" type="text" placeholder="Search">
                                        <button class="btn btn-secondary my-2 my-sm-0" type="submit">Search</button>
                                    </form>
                                </li>
                                <li class="nav-item dropdown">
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
                            </ul>
                        </div>
                    </div>
                    <div v-if="!isAuth">
                        <div class="my-2 my-lg-0">
                            <ul class="navbar-nav mr-auto">
                                <li class="nav-item">
                                    <b-button variant="primary" :to="{ name: 'login' }" class="nav-link">Log in</b-button>
                                </li>
                                <li class="nav-item">
                                    <b-button variant="info" :to="{ name: 'register' }" class="nav-link">Sign Up</b-button>
                                </li>
                            </ul>
                        </div>
                    </div>
                </div>
            </div>
        </nav>
    </div>
</template>

<script>
    export default {
        data() {
            return {
                credsLoaded: false
            }
        },
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
        },
        mounted() {
            this.$store.dispatch("getUserInfo")
                .then(response => {
                    this.credsLoaded = true;
                    resolve(response)
                });
        }
    }
</script>

<style scoped>
    .navbar-nav .nav-item {
        margin-left: 5px;
        margin-right: 5px;
    }
</style>
