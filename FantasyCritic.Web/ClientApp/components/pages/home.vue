<template>
    <div>
        This is the home page! It is {{ time }}!
        <b-button variant="info" v-on:click="clear()" class="nav-link">Clear</b-button>
        <b-button variant="info" v-on:click="fetchViewModel()" class="nav-link">Fetch</b-button>
    </div>
</template>

<script>
import Vue from "vue";
import axios from "axios";
export default {
        data() {
            return {
                time: "",
                error: ""
            }

        },
        methods: {
            fetchViewModel() {
                axios
                    .get('/api/Account/time/')
                    .then((res) => {
                        this.time = res.data;
                    })
                    .catch(error => {
                        this.error = error;
                    });
            },
            clear() {
                this.time = "";
            }
        },
        mounted() {
            this.fetchViewModel();
        }
}
</script>
