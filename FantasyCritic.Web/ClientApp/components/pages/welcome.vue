<template>
  <div>
    <div class="row hero-area">
      <div class="col-lg-4 offset-lg-1 col-md-12">
        <h2>Fantasy Football...for video games!</h2>
        <p>
          Fantasy Critic is a game where you get to play as your own "fantasy" video game publisher. It's your job to choose the best reviewing games to publish!
        </p>
      </div>
      <div class="col-lg-4 offset-lg-2 col-md-12">
        <h2>Sign up for free</h2>

        <registerForm></registerForm>
      </div>
    </div>

    <div class="row welcome-area">
      <div class="col-sm-12 col-md-4 offset-md-2 welcome-heading">Play free forever!</div>
      <p class="col-sm-12 col-md-4 welcome-message">
        FantasyCritic is like fantasy football...but for video games! You and several friends will create your own virtual video game “Publishers”,
        and then you will assemble a roster of the games you believe will review the best. Similar to fantasy football, the players will alternate
        picking games in a snake draft style to start the year. At the end of the year, the winner is the team with the best lineup of video games
        based on scores from <a href="https://opencritic.com/">opencritic.com</a>.
      </p>
    </div>

    <div class="row">
      <div class="col-sm-12 col-md-4 offset-md-2">
        <img src="/images/monitor-mock.png" />
      </div>
      <div class="col-sm-12 col-md-4">
        <h3>How to get Started</h3>
        <div class="starting-step">
          <icon icon="trophy" />
          <span>Create a League</span>
        </div>
        <div class="starting-step">
          <icon icon="user-friends" />
          <span>Invite your friends</span>
        </div>
        <div class="starting-step">
          <icon icon="list-ol" />
          <span>Draft your roster</span>
        </div>
      </div>
    </div>
    <h3>Features and Highlights</h3>
    <div class="row">
      <div class="feature-highlight col-3 offset-4">
        <icon icon="user-friends" />
        <span>Compete with your friends</span>
      </div>
      <div class="feature-highlight col-3">
        <icon icon="list-ol" />
        <span>Assemble your dream team</span>
      </div>
    </div>
    <div class="row">
      <div class="feature-highlight col-3 offset-4">
        <icon icon="trophy" />
        <span>Win big (bragging rights)</span>
      </div>
      <div class="feature-highlight col-3">
        <icon icon="dollar-sign" />
        <span>Totally free</span>
      </div>
    </div>
    <div class="row">
      <div class="col-lg-6 col-md-12">
        <h3>Frequently Asked Questions</h3>
        <homeFaq></homeFaq>
        <div class="alert alert-secondary">
          <div class="d-flex">
            <div class="mr-auto p-2">More Questions?</div>
            <div><b-button class="faq-link mx-2" variant="info" :to="{ name: 'faq' }">View our full FAQ Page</b-button></div>
          </div>
        </div>
      </div>
      <div class="col-lg-6 col-md-12">
        <h3>Recent Tweets</h3>
        <a class="twitter-timeline" href="https://twitter.com/fantasy_critic?ref_src=twsrc%5Etfw">Tweets by fantasy_critic</a>
      </div>
    </div>
  </div>
</template>

<script>
    import Vue from 'vue';
    import axios from 'axios';
    import RegisterForm from "components/modules/registerForm";
    import HomeFaq from "components/modules/homeFaq";

    export default {
      components: {
        RegisterForm,
        HomeFaq
      }
    }
</script>
<style scoped>
  .hero-area {
    background-color: #595959;
  }
  .registration-area{
    float: right;
  }
  .welcome-area{
    background-color: grey;
  }
  .welcome-heading {
  }
  .welcome-message {
  }
</style>
