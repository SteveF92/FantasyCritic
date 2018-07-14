<template>
    <div>
        This is the home page! You are {{ userName }}!
    </div>
</template>

<script>
import Vue from "vue";
import axios from "axios";
export default {
        computed: {
            userName() {
                return this.$store.getters.userInfo.userName
            }
        }
}
</script>
