<template>
  <div>
    <div class="row hero-area">
      <div class="col-lg-6 col-md-12 hero-message-area">
        <div class="hero-message">
          <h1 class="hero-message-header shadowed">
            Fantasy Football...
            <br />
            for video games!
          </h1>
          <p class="shadowed hero-message-details">
            Fantasy Critic is a game where you get to play as your own "fantasy" video game publisher. It's your job to choose the best reviewing games to publish!
          </p>
        </div>
      </div>
      <div class="col-lg-6 col-md-12 registration-area">
        <div class="registration-box shadowed">
          <h3>Sign up for free</h3>
          <registerForm></registerForm>
        </div>
      </div>
    </div>

    <div class="row welcome-area">
      <div class="col-lg-6 col-md-12 welcome-heading">
        <h2>Play free forever!</h2>
      </div>
      <div class="col-lg-6 col-md-12">
        <p class="welcome-message">
          FantasyCritic is like fantasy football...but for video games! You and several friends will create your own virtual video game “Publishers”,
          and then you will assemble a roster of the games you believe will review the best. Similar to fantasy football, the players will alternate
          picking games in a snake draft style to start the year. At the end of the year, the winner is the team with the best lineup of video games
          based on scores from <a href="https://opencritic.com/">opencritic.com</a>.
        </p>
      </div>
    </div>

    <div class="row">
      <div class="col-lg-6 col-md-12 computer-image-area">
        <img src="/images/monitor-mock.png" class="computer-image" />
      </div>
      <div class="col-lg-6 col-md-12 get-started-area">
        <div class="get-started-message">
          <h3>How to get Started</h3>
        </div>

        <div class="starting-steps">
          <div class="starting-step">
            <font-awesome-layers class="fa-3x">
              <font-awesome-icon :icon="['far', 'circle']" />
              <font-awesome-icon icon="trophy" transform="shrink-8 left-1" :style="{ color: 'd6993a' }" />
            </font-awesome-layers>
            <span>Create a League</span>
          </div>
          <div class="starting-step">
            <font-awesome-layers class="fa-3x">
              <font-awesome-icon :icon="['far', 'circle']" />
              <font-awesome-icon icon="user-friends" transform="shrink-9 left-2" :style="{ color: 'd6993a' }" />
            </font-awesome-layers>
            <span>Invite your friends</span>
          </div>
          <div class="starting-step">
            <font-awesome-layers class="fa-3x">
              <font-awesome-icon :icon="['far', 'circle']" />
              <font-awesome-icon icon="list-ol" transform="shrink-8" :style="{ color: 'd6993a' }" />
            </font-awesome-layers>
            <span>Draft your roster</span>
          </div>
        </div>
      </div>
    </div>
    <h3>Features and Highlights</h3>
    <div class="row">
      <div class="feature-highlight col-3 offset-4">
        <font-awesome-icon icon="user-friends" />
        <span>Compete with your friends</span>
      </div>
      <div class="feature-highlight col-3">
        <font-awesome-icon icon="list-ol" />
        <span>Assemble your dream team</span>
      </div>
    </div>
    <div class="row">
      <div class="feature-highlight col-3 offset-4">
        <font-awesome-icon icon="trophy" />
        <span>Win big (bragging rights)</span>
      </div>
      <div class="feature-highlight col-3">
        <font-awesome-icon icon="dollar-sign" />
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
    background-color: #414141;
  }
  .hero-message-area {
    display: flex; /* establish flex container */
    flex-direction: column; /* make main axis vertical */
    justify-content: center; /* center items vertically, in this case */
    align-items: center; /* center items horizontally, in this case */
  }
  .hero-message {
    width: 60%;
    min-width: 400px;
  }
  .hero-message-header {
  }
  .hero-message-details {
  }

  .registration-area {
    padding-top: 35px;
    padding-bottom: 35px;
    display: flex; /* establish flex container */
    flex-direction: column; /* make main axis vertical */
    align-items: center; /* center items horizontally, in this case */
  }
  .registration-box {
    width: 50%;
    min-width: 400px;
  }

  .welcome-area {
    background-color: #4A4A4A;
  }

  .welcome-heading {
    text-align: right;
  }

  .welcome-heading h2 {
    color: black;
    margin-top: 20px;
    margin-right: 75px;
  }
  .welcome-message {
    width: 70%;
  }

  .computer-image-area {
    display: flex; /* establish flex container */
    flex-direction: column; /* make main axis vertical */
    justify-content: center; /* center items vertically, in this case */
    align-items: center; /* center items horizontally, in this case */
  }

  .computer-image {

  }

  .get-started-area{
    margin-top: 20px;
  }
</style>
