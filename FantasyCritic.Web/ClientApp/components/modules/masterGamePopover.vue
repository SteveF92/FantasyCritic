<template>
  <div v-if="masterGame">
    <img v-if="masterGame.boxartFileName" :src="boxartLink" alt="Cover Image" class="game-image">
    <div class="game-description">
      <h5>{{masterGame.gameName}}</h5>

    </div>
    <!--<p>
      <strong>Release Date: </strong>
      <span v-if="masterGame.releaseDate">{{releaseDate(masterGame)}}</span>
      <span v-else>{{masterGame.estimatedReleaseDate}} (Estimated)</span>
    </p>
    <p>
      <strong>Critic Score: </strong>
      {{masterGame.criticScore | score(2)}}
      <span v-if="masterGame.averagedScore">(Averaged Score)</span>
    </p>
    <p v-if="masterGame.openCriticID">
      <a :href="openCriticLink(masterGame)" target="_blank">Open Critic Link <icon icon="external-link-alt" size="xs" /></a>
    </p>

    <div v-if="masterGame.subGames && masterGame.subGames.length > 0">
      <h3>Sub Games (Episodes)</h3>
      <div v-for="subGame in masterGame.subGames">
        <h4>{{subGame.gameName}}</h4>
        <p>
          <strong>Release Date: </strong>
          <span v-if="subGame.releaseDate">{{releaseDate(subGame)}}</span>
          <span v-else>{{subGame.estimatedReleaseDate}} (Estimated)</span>
        </p>
        <p>
          <strong>Critic Score: </strong>
          {{subGame.criticScore | score(2)}}
        </p>
        <p v-if="subGame.openCriticID">
          <a :href="openCriticLink(subGame)" target="_blank">Open Critic Link <icon icon="external-link-alt" size="xs" /></a>
        </p>
      </div>
    </div>-->
  </div>
</template>

<script>
  import Vue from "vue";
  import axios from "axios";
  import moment from "moment";

  export default {
    data() {
      return {
        masterGame: null,
        error: ""
      }
    },
    props: ['mastergameid'],
    computed: {
      boxartLink() {
        return "https://s3.amazonaws.com/fantasy-critic-box-art/" + this.masterGame.boxartFileName;
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
</style>
