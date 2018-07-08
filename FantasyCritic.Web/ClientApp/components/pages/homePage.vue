<template>
    <div>
        {{message}}
    </div>
</template>

<script>
import Vue from "vue";
import axios from "axios";
export default {
    data() {
        return {
            message: "This is the home page"
        }
    }
}
</script>
