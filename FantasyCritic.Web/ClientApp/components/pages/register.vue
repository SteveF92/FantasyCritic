<template>
    <div>
        <h2>Register</h2>
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
                error: ""
            }
        }
    }
</script>
