<template>
  <div>
    <div class="col-md-12 col-lg-10 offset-md-0 offset-lg-1">
      <div class="row hero-area">
        <div class="col-md-5 offset-md-1 col-sm-12 hero-message-area">
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
        <div class="col-md-6 col-sm-12 registration-area">
          <div class="registration-box shadowed">
            <h2>Sign up for free</h2>
            <registerForm></registerForm>
          </div>
        </div>
      </div>
    </div>

    <div class="row welcome-area">
      <div class="col-md-5 offset-md-1 col-sm-12 welcome-heading">
        <h1>Play free forever!</h1>
      </div>
      <div class="col-md-6 col-sm-12">
        <p class="welcome-message">
          Fantasy Critic is like fantasy football...but for video games! You and several friends will create your own virtual video game “Publishers”,
          and then you will assemble a roster of the games you believe will review the best. Similar to fantasy football, the players will alternate
          picking games in a snake draft style to start the year. At the end of the year, the winner is the team with the best lineup of video games
          based on scores from <a href="https://opencritic.com/" target="_blank">opencritic.com <font-awesome-icon icon="external-link-alt" size="sm" /></a>.
        </p>
      </div>
    </div>

    <br />

    <div class="row welcome-area">
      <div class="col-md-5 offset-md-1 col-sm-12 welcome-heading">
        <h1>
          New Mode:
          <br />
          Critics Royale
        </h1>
      </div>
      <div class="col-md-6 col-sm-12">
        <p class="welcome-message">
          Don't have a league to join? Want to start playing in the middle of the year? Our new "Critics Royale" mode is structured similar to Daily Fantasy Sports, allowing you to compete on your own against
          the entire site for a leaderboard position. Instead of drafting, you're given a 'budget' which you use to select your games.
          Your goal is to spend that money wisely and put together the best lineup of games that you can.
        </p>
      </div>
    </div>

    <div class="row justify-content-center">
      <div class="col-xl-4 offset-xl-1 col-lg-5 offset-lg-1 col-md-0 offset-md-0 computer-image-area">
        <img src="/images/monitor-mock.png" class="computer-image" />
      </div>
      <div class="col-lg-5 col-md-12 get-started-area">
        <div class="get-started-message">
          <h2>How to get Started</h2>
        </div>

        <div class="starting-steps">
          <div class="starting-step">
            <font-awesome-layers class="fa-4x">
              <font-awesome-icon :icon="['far', 'circle']" />
              <font-awesome-icon icon="trophy" transform="shrink-8 left-1" :style="{ color: 'd6993a' }" />
            </font-awesome-layers>
            <div class="starting-step-text">
              <span class="step-number-text">Step 1:</span>
              <br />
              <span>Create a League</span>
            </div>
          </div>
          <div class="starting-step">
            <font-awesome-layers class="fa-4x">
              <font-awesome-icon :icon="['far', 'circle']" />
              <font-awesome-icon icon="user-friends" transform="shrink-9 left-2" :style="{ color: 'd6993a' }" />
            </font-awesome-layers>
            <div class="starting-step-text">
              <span class="step-number-text">Step 2:</span>
              <br />
              <span>Invite your friends</span>
            </div>
          </div>
          <div class="starting-step">
            <font-awesome-layers class="fa-4x">
              <font-awesome-icon :icon="['far', 'circle']" />
              <font-awesome-icon icon="list-ol" transform="shrink-8" :style="{ color: 'd6993a' }" />
            </font-awesome-layers>
            <div class="starting-step-text">
              <span class="step-number-text">Step 3:</span>
              <br />
              <span>Draft your roster</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="col-md-12 col-lg-10 offset-md-0 offset-lg-1 features-area">
      <h2 class="features-heading">Features and Highlights</h2>
      <div class="feature-highlights-area">
        <div class="row">
          <div class="col-sm-12 col-md-6">
            <div class="feature-highlight shadowed">
              <font-awesome-layers class="fa-4x">
                <font-awesome-icon :icon="['far', 'circle']" />
                <font-awesome-icon icon="user-friends" transform="shrink-9 left-2" :style="{ color: 'd6993a' }" />
              </font-awesome-layers>
              <span class="feature-description">Compete with your friends</span>
            </div>
          </div>
          <div class="col-sm-12 col-md-6">
            <div class="feature-highlight shadowed">
              <font-awesome-layers class="fa-4x">
                <font-awesome-icon :icon="['far', 'circle']" />
                <font-awesome-icon icon="list-ol" transform="shrink-8" :style="{ color: 'd6993a' }" />
              </font-awesome-layers>
              <span class="feature-description">Assemble your dream team</span>
            </div>
          </div>
          <div class="col-sm-12 col-md-6">
            <div class="feature-highlight shadowed">
              <font-awesome-layers class="fa-4x">
                <font-awesome-icon :icon="['far', 'circle']" />
                <font-awesome-icon icon="trophy" transform="shrink-8 left-1" :style="{ color: 'd6993a' }" />
              </font-awesome-layers>
              <span class="feature-description">Win big (bragging rights)</span>
            </div>
          </div>
          <div class="col-sm-12 col-md-6">
            <div class="feature-highlight shadowed">
              <font-awesome-layers class="fa-4x">
                <font-awesome-icon :icon="['far', 'circle']" />
                <font-awesome-icon icon="money-bill-alt" transform="shrink-8 left-2" :style="{ color: 'd6993a' }" />
                <font-awesome-icon icon="slash" transform="shrink-6 left-2" />
              </font-awesome-layers>
              <span class="feature-description">Totally free</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="row">
      <div class="col-lg-5 col-md-12 offset-lg-1 col-md-0">
        <h2>Frequently Asked Questions</h2>
        <div class="full-welcome">
          <homeFaq></homeFaq>
          <div class="alert alert-secondary">
            <div class="d-flex">
              <div class="mr-auto p-2">More Questions?</div>
              <b-button class="nav-link" variant="primary" :to="{ name: 'faq' }">View our full FAQ Page</b-button>
            </div>
          </div>
        </div>
        <div class="minimal-welcome">
          <div class="alert alert-secondary">
            <b-button class="nav-link" variant="primary" :to="{ name: 'faq' }">View our FAQ Page</b-button>
          </div>
        </div>
      </div>

      <div class="col-lg-5 col-md-10 offset-lg-0 offset-md-1">
        <div class="tweet-section">
          <tweets></tweets>
        </div>
      </div>
    </div>

    <div class="col-md-12 col-lg-10 offset-md-0 offset-lg-1">
      <h2>So far we have...</h2>
    </div>
    <siteCounts></siteCounts>
  </div>
</template>

<script>
  import Vue from 'vue';
  import axios from 'axios';
  import SiteCounts from "components/modules/siteCounts";
  import RegisterForm from "components/modules/registerForm";
  import HomeFaq from "components/modules/homeFaq";
  import Tweets from "components/modules/tweets";

  export default {
    components: {
      RegisterForm,
      HomeFaq,
      Tweets,
      SiteCounts
    }
  }
</script>
<style scoped>

  @media only screen and (max-width: 679px) {
    .full-welcome {
      display: none;
    }
  }

  @media only screen and (min-width: 680px) {
    .minimal-welcome {
      display: none;
    }
  }

  @media only screen and (max-width: 992px) {
    .computer-image-area {
      display: none;
    }
  }

  .hero-message-area {
    display: flex; /* establish flex container */
    flex-direction: column; /* make main axis vertical */
    justify-content: center; /* center items vertically, in this case */
    align-items: center; /* center items horizontally, in this case */
  }
  .hero-message {

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
    width: 100%;
    max-width: 500px;
  }

  .welcome-area {
    background-color: #4A4A4A;
  }

  .welcome-heading {
    text-align: center;
  }

  .welcome-message {
    max-width: 700px;
  }

  .computer-image-area{
    text-align: center;
  }

  .computer-image {
    margin-top: 10px;
    margin-bottom: 10px;
  }

  .get-started-area {
    margin-top: 20px;
    display: flex;
    flex-direction: column;
    align-items: center;
  }

  .starting-step {
    display: flex; /* establish flex container */
    flex-direction: row; /* make main axis vertical */
    margin-bottom: 25px;
  }

  .starting-step-text{
    margin-left: 10px;
  }
  .step-number-text {
    font-weight: bold;
    text-transform: uppercase;
    font-family: monospace;
    font-size: 17px;
  }

  .features-area {
    background-color: #414141;
    padding-bottom: 15px;
  }

  .features-heading {
    text-align: center;
  }

  /*.feature-highlights {
    width: 50%;
    min-width: 300px;
    display: flex;
    flex-wrap: wrap;
  }*/

  .feature-highlight {
    margin: 2px;
    display: flex; /* establish flex container */
    align-items: center; /* center items horizontally, in this case */
  }

  .feature-description {
    font-weight: bold;
    padding-left: 7px;
    vertical-align: middle;
  }

  .faq-button {
    text-align: center;
  }

  .tweet-section {
    margin-top: 15px;
  }
</style>
