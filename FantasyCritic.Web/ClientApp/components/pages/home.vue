<template>
    <div>
        Welcome to Fantasy Critic!

        <router-link :to="{ name: 'createLeague' }" class="nav-link" title="CreateLeague">Create a League</router-link>
    </div>
</template>

<script>
import Vue from "vue";
import axios from "axios";
export default {
        data() {
            return {

            }

        },
        methods: {

        },
        mounted() {
        }
}
</script>
