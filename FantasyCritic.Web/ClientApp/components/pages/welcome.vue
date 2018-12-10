<template>
  <div>
    <div class="row">
      <div class="col-lg-4 offset-lg-1 col-md-12">
        <h2>Welcome to Fantasy Critic</h2>
        <p>
          FantasyCritic is like fantasy football...but for video games! You and several friends will create your own virtual video game “Publishers”,
          and then you will assemble a roster of the games you believe will review the best. Similar to fantasy football, the players will alternate
          picking games in a snake draft style to start the year. At the end of the year, the winner is the team with the best lineup of video games
          based on scores from <a href="https://opencritic.com/">opencritic.com</a>.
        </p>
      </div>
      <div class="col-lg-4 offset-lg-2 col-md-12">
        <h2>Sign up for free</h2>

        <registerForm></registerForm>
      </div>
    </div>
  </div>
</template>
<script>
    import Vue from 'vue';
    import axios from 'axios';
    import RegisterForm from "components/modules/registerForm";

    export default {
      components: {
        RegisterForm
      }
    }
</script>
<style scoped>
  .registration-area{
    float:right;
  }
</style>
