<template>
  <div>
    <div v-if="masterGame" class="col-md-10 offset-md-1 col-sm-12">
      <h1>{{masterGame.gameName}}</h1>
      <div class="game-image-area">
        <img v-show="this.masterGame.boxartFileName" :src="boxartLink" alt="Cover Image" class="game-image">
        <font-awesome-layers v-show="!this.masterGame.boxartFileName" class="fa-8x no-game-image">
          <font-awesome-icon :icon="['far', 'square']" />
          <font-awesome-layers-text transform="shrink-14" value="No image found" />
        </font-awesome-layers>
      </div>

      <div>
        <router-link class="text-primary" :to="{ name: 'masterGameChangeRequest', query: { mastergameid: masterGame.masterGameID }}"><strong>Suggest a correction</strong></router-link>
      </div>

      <masterGameDetails :masterGame="masterGame" class="text-well"></masterGameDetails>

      <div v-if="masterGame.subGames && masterGame.subGames.length > 0">
        <h2>Sub Games (Episodes)</h2>
        <div v-for="subGame in masterGame.subGames">
          <h3>{{subGame.gameName}}</h3>
          <p>
            <strong>Release Date: </strong>
            <span v-if="subGame.releaseDate">{{releaseDate(subGame)}}</span>
            <span v-else>{{subGame.estimatedReleaseDate}} (Estimated)</span>
          </p>
          <p v-if="subGame.openCriticID">
            <a :href="openCriticLink(subGame)" target="_blank">Open Critic Link <font-awesome-icon icon="external-link-alt" size="xs" /></a>
          </p>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import Vue from 'vue';
import axios from 'axios';
import moment from 'moment';
import MasterGameDetails  from '@/components/modules/masterGameDetails';

export default {
  data() {
    return {
      masterGame: null,
      error: ''
    };
  },
  props: ['mastergameid'],
  components: {
    MasterGameDetails
  },
  computed: {
    boxartLink() {
      if (this.masterGame.boxartFileName) {
        return 'https://s3.amazonaws.com/fantasy-critic-box-art/' + this.masterGame.boxartFileName;
      }
      return null;
    }
  },
  methods: {
    fetchMasterGame() {
      axios
        .get('/api/game/MasterGame/' + this.mastergameid)
        .then(response => {
          this.masterGame = response.data;
        })
        .catch(returnedError => (this.error = returnedError));
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
};
</script>
