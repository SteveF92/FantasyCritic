<template>
    <div>
        <nav class="navbar navbar-expand-lg navbar-dark bg-dark">
            <router-link :to="{ name: 'homePage' }" class="navbar-brand" title="Fantasy Critic">Fantasy Critic</router-link>
            <button class="navbar-toggler collapsed" type="button" data-toggle="collapse" data-target="#navbarColor02" aria-controls="navbarColor02" aria-expanded="false" aria-label="Toggle navigation" style="">
                <span class="navbar-toggler-icon"></span>
            </button>

            <div class="navbar-collapse collapse" id="navbarColor02" style="">
                <ul class="navbar-nav mr-auto">
                    <li class="nav-item">
                        <router-link :to="{ name: 'aboutPage' }" class="nav-link" title="About">About</router-link>
                    </li>
                    <li class="nav-item">
                        <router-link :to="{ name: 'contactPage' }" class="nav-link" title="Contact">Contact</router-link>
                    </li>
                </ul>
                <form class="form-inline my-2 my-lg-0">
                    <input class="form-control mr-sm-2" type="text" placeholder="Search">
                    <button class="btn btn-secondary my-2 my-sm-0" type="submit">Search</button>
                </form>
                <div class="my-2 my-lg-0">
                    <ul class="navbar-nav mr-auto">
                        <li class="nav-item">
                            <b-button variant="primary":to="{ name: 'login' }" class="nav-link">Log in</b-button>
                        </li>
                    </ul>
                </div>
            </div>
        </nav>
    </div>
</template>

<script>
    export default {

    }
</script>

<style scoped>
</style>
