<template>
    <div>
        <h2>About</h2>
        <hr />
        <div class="alert alert-danger" v-if="error">An error has occurred.</div>
        <p>{{description}}</p>
    </div>
</template>

<script>
    import Vue from 'vue';
    import axios from 'axios';

    export default {
        data() {
            return {
                description: "Fantasy critic is a thing...",
                error: ""
            }

        }
    }
</script>
