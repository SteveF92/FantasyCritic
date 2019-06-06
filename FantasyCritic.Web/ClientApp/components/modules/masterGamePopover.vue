<template>
  <div>
    <div v-if="masterGame">
      <popper ref="gamePopoverRef" trigger="click" :options="{ placement: 'top', modifiers: { offset: { offset: '0,0px' } }}" v-on:show="newPopoverShown">
        <div class="popper">
          <div class="game-image-area">
            <img v-show="this.masterGame.boxartFileName" :src="boxartLink" alt="Cover Image" class="game-image">
            <font-awesome-layers v-show="!this.masterGame.boxartFileName" class="fa-8x no-game-image">
              <font-awesome-icon :icon="['far', 'square']" />
              <font-awesome-layers-text transform="shrink-14" value="No image found" />
            </font-awesome-layers>
          </div>

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
            <div v-if="masterGame.openCriticID">
              <a :href="openCriticLink(masterGame)" target="_blank"><strong>OpenCritic Link <font-awesome-icon icon="external-link-alt" /></strong></a>
            </div>
            <div>
              <router-link class="text-primary" :to="{ name: 'mastergame', params: { mastergameid: masterGame.masterGameID }}"><strong>View full details</strong></router-link>
            </div>
            <div>
              <router-link class="text-primary" :to="{ name: 'masterGameChangeRequest', query: { mastergameid: masterGame.masterGameID }}"><strong>Suggest a correction</strong></router-link>
            </div>
          </div>
          <div class="close-button fake-link" v-on:click="closePopover">
            <font-awesome-icon icon="times" size="lg" :style="{ color: 'd6993a' }"/>
          </div>
        </div>

        <span slot="reference" class="text-primary fake-link">
          {{masterGame.gameName}}
        </span>
      </popper>
    </div>
  </div>
</template>

<script>
  import Vue from "vue";
  import axios from "axios";
  import moment from "moment";
  import Popper from 'vue-popperjs';
  import 'vue-popperjs/dist/css/vue-popper.css';

  export default {
    data() {
      return {
        error: ""
      }
    },
    components: {
      'popper': Popper,
    },
    props: ['masterGame'],
    computed: {
      boxartLink() {
        if (this.masterGame.boxartFileName) {
          return "https://s3.amazonaws.com/fantasy-critic-box-art/" + this.masterGame.boxartFileName;
        }
        return null;
      }
    },
    methods: {
      releaseDate(game) {
        return moment(game.releaseDate).format('MMMM Do, YYYY');
      },
      openCriticLink(game) {
        return "https://opencritic.com/game/" + game.openCriticID + "/a";
      },
      closePopover() {
        this.$refs.gamePopoverRef.doClose();
      },
      newPopoverShown() {
        this.$emit('newPopoverShown', this.masterGame);
      }
    }
  }
</script>
<style scoped>
  .popper {
    min-width: 370px;
    display: flex;
    flex-direction: row;
    flex-wrap: nowrap;
    justify-content: space-evenly;
    background-color: #e6e6e6;
    align-items: center;
  }
  .game-image-area {
  }
  .game-image {
    max-width: 150px;
    max-height: 125px;
  }
  .no-game-image {

  }
  .game-description{
    margin-left: 5px;
    margin-right: 20px;
  }

  .close-button {
    position: absolute;
    right: 5px;
    top: 0px;
  }

  .game-name {
    text-wrap: normal;
    text-align: center;
  }
</style>
