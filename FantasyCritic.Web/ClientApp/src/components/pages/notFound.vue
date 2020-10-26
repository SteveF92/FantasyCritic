<template>
    <div>
      <div class="col-md-10 offset-md-1 col-sm-12">
        <h1>Woops!</h1>
        <h2>There isn't a page here.</h2>
        <b-button variant="primary" :to="{ name: 'welcome' }">Go back home</b-button>
      </div>
    </div>
</template>

<script>
import Vue from 'vue';

export default {
    data() {
        return {
        };
    }
};
</script>
