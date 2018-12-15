<template>
  <div>
    <div v-if="masterGame">
      <popper ref="gamePopoverRef" trigger="click" :options="{ placement: 'top', modifiers: { offset: { offset: '0,10px' } }}">
        <div class="popper">

          <div class="close-button fake-link" v-on:click="closePopover">X</div>
          <img :src="boxartLink" alt="Cover Image" class="game-image">

          <div class="game-description">
            <h5>{{masterGame.gameName}}</h5>
            <div>
              <strong>Release Date: </strong>
              <span v-if="masterGame.releaseDate">{{releaseDate(masterGame)}}</span>
              <span v-else>{{masterGame.estimatedReleaseDate}} (Estimated)</span>
            </div>
            <div>
              <strong>Critic Score: </strong>
              {{masterGame.criticScore | score(2)}}
              <span v-if="masterGame.averagedScore">(Averaged Score)</span>
            </div>
            <div v-if="masterGame.openCriticID">
              <a :href="openCriticLink(masterGame)" target="_blank">OpenCritic Link <icon icon="external-link-alt" size="xs" /></a>
            </div>
            <router-link class="text-primary" :to="{ name: 'mastergame', params: { mastergameid: masterGame.masterGameID }}">View full details</router-link>
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
        masterGame: null,
        error: ""
      }
    },
    components: {
      'popper': Popper,
    },
    props: ['mastergameid'],
    computed: {
      boxartLink() {
        if (this.masterGame.boxartFileName) {
          return "https://s3.amazonaws.com/fantasy-critic-box-art/" + this.masterGame.boxartFileName;
        }
        return "https://s3.amazonaws.com/fantasy-critic-box-art/noBoxArt.png";
      }
    },
    methods: {
      releaseDate(game) {
        return moment(game.releaseDate).format('MMMM Do, YYYY');
      },
      openCriticLink(game) {
        return "https://opencritic.com/game/" + game.openCriticID + "/";
      },
      fetchMasterGame() {
        axios
          .get('/api/game/MasterGame/' + this.mastergameid)
          .then(response => {
            this.masterGame = response.data;
          })
          .catch(returnedError => (this.error = returnedError));
      },
      closePopover() {
        this.$refs.gamePopoverRef.doClose();
      }
    },
    mounted() {
      this.fetchMasterGame();
    },
    watch: {
      '$route'(to, from) {
        this.fetchMasterGame();
      }
    }
  }
</script>
<style scoped>
  .game-image {
    float: left;
    width: 125px;
  }
  .game-description{
    float:left;
  }
  .close-button {
    float: right;
  }
  .fake-link {
    text-decoration: underline;
    cursor: pointer;
  }
</style>
