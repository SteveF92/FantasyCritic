<template>
  <div>
    <div v-if="masterGame" class="summary">
      <div class="row">
        <div class="col-lg-6 col-md-12 game-image-area">
          <img v-show="this.masterGame.boxartFileName" :src="boxartLink" alt="Cover Image" class="game-image">
          <font-awesome-layers v-show="!this.masterGame.boxartFileName" class="fa-8x no-game-image">
            <font-awesome-icon :icon="['far', 'square']" />
            <font-awesome-layers-text transform="shrink-14" value="No image found" />
          </font-awesome-layers>
        </div>

        <div class="col-lg-6 col-md-12">
          <div class="game-description">
            <h4 class="game-name">{{masterGame.gameName}}</h4>
            <div>
              <strong>Release Date: </strong>
              <span v-if="masterGame.releaseDate">{{releaseDate(masterGame)}}</span>
              <span v-else>{{masterGame.estimatedReleaseDate}} (Estimated)</span>
            </div>
            <div>
              <strong>Percent Published: </strong>
              {{masterGame.percentStandardGame | percent(1)}}
            </div>
            <div>
              <strong>Percent Counterpicked: </strong>
              {{masterGame.percentCounterPick | percent(1)}}
            </div>
            <div>
              <strong>Average Draft Position: </strong>
              <span v-show="masterGame.averageDraftPosition">{{masterGame.averageDraftPosition | score(1)}}</span>
              <span v-show="!masterGame.averageDraftPosition">Undrafted</span>
            </div>
            <div>
              <strong>Hype Factor: </strong>
              <span v-show="masterGame.dateAdjustedHypeFactor">{{masterGame.dateAdjustedHypeFactor | score(1)}}</span>
              <span v-show="!masterGame.dateAdjustedHypeFactor">Unhyped...</span>
            </div>
            <div>
              <strong v-if="!masterGame.criticScore">Projected Points: </strong>
              <strong v-else>Pre-Release Projected Points: </strong>
              <span v-show="masterGame.projectedFantasyPoints">~{{masterGame.projectedFantasyPoints | score(1)}}</span>
            </div>
            <div v-if="masterGame.openCriticID">
              <a :href="openCriticLink(masterGame)" target="_blank"><strong>OpenCritic Link <font-awesome-icon icon="external-link-alt" /></strong></a>
            </div>
            <div>
              <router-link class="text-primary" :to="{ name: 'mastergame', params: { mastergameid: masterGame.masterGameID }}"><strong>View full details</strong></router-link>
            </div>
            <div>
              <router-link class="text-primary" :to="{ name: 'masterGameChangeRequest', query: { mastergameid: masterGame.masterGameID }}"><strong>Suggest a correction</strong></router-link>
            </div>
            <div v-if="isAdmin">
              <router-link class="text-primary" :to="{ name: 'masterGameEditor', params: { mastergameid: masterGame.masterGameID }}"><strong>Edit Master Game</strong></router-link>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import Vue from 'vue';
import axios from 'axios';
import moment from 'moment';

export default {
  data() {
    return {
      error: ''
    };
  },
  props: ['masterGame'],
  computed: {
    boxartLink() {
      if (this.masterGame.boxartFileName) {
        return 'https://s3.amazonaws.com/fantasy-critic-box-art/' + this.masterGame.boxartFileName;
      }
      return null;
    },
    isAdmin() {
      return this.$store.getters.isAdmin;
    }
  },
  methods: {
    releaseDate(game) {
      return moment(game.releaseDate).format('MMMM Do, YYYY');
    },
    openCriticLink(game) {
      return 'https://opencritic.com/game/' + game.openCriticID + '/a';
    }
  }
};
</script>
<style scoped>
  .summary {
    min-width: 370px;
    display: flex;
    flex-direction: row;
    flex-wrap: nowrap;
    justify-content: space-evenly;
    align-items: center;
    color: black;
  }

  .game-image-area {
    margin: auto;
  }

  .game-image {
    display: block;
    margin: auto;
    max-width: 300px;
    max-height: 250px;
  }

  .no-game-image {
  }

  .game-description {
    margin-left: 5px;
    margin-right: 20px;
  }

  .game-name {
    text-wrap: normal;
    text-align: center;
  }
</style>
