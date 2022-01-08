<template>
  <div>
    <div v-if="masterGame" class="col-md-10 offset-md-1 col-sm-12">
      <h1>{{masterGame.gameName}}</h1>
      <div class="game-image-area">
        <div v-if="masterGame.ggToken && masterGame.ggCoverArtFileName">
          <img v-show="masterGame.ggCoverArtFileName" :src="ggCoverArtLink" alt="Cover Image" class="game-image">
          <a :href="ggLink" target="_blank"><strong>Image Provided by GG|<font-awesome-icon icon="external-link-alt" /></strong></a>
        </div>
        <font-awesome-layers v-show="!masterGame.ggCoverArtFileName" class="fa-8x no-game-image">
          <font-awesome-icon :icon="['far', 'square']" />
          <font-awesome-layers-text transform="shrink-14" value="No image found" />
        </font-awesome-layers>
      </div>

      <div>
        <router-link class="text-primary" :to="{ name: 'masterGameChangeRequest', query: { mastergameid: masterGame.masterGameID }}"><strong>Suggest a correction</strong></router-link>
      </div>
      <div v-if="isAdmin">
        <router-link class="text-primary" :to="{ name: 'masterGameEditor', params: { mastergameid: masterGame.masterGameID }}"><strong>Edit Master Game</strong></router-link>
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
    ggCoverArtLink() {
      if (this.masterGame.ggCoverArtFileName) {
        return `https://ggapp.imgix.net/media/games/${this.masterGame.ggToken}/${this.masterGame.ggCoverArtFileName}?w=307&dpr=1&fit=crop&auto=compress&q=95`;
      }
      return null;
    },
    ggLink() {
      return `https://ggapp.io/games/${this.masterGame.ggToken}`;
    },
    isAdmin() {
      return this.$store.getters.isAdmin;
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
<style scoped>
  .game-image {
    border-radius: 5%;
  }
</style>
