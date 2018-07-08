<template>
    <div>
        <h2>Login</h2>
        <hr />
    </div>
</template>

<script>
    import Vue from 'vue';
    import axios from 'axios';

    export default {
        data() {
            return {
                userName: "",
                password: "",
                rememberMe: false,
                isBusy: false,
                error: ""
            }
        }
    }
</script>
