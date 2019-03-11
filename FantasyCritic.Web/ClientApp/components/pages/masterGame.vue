<template>
  <div v-if="masterGame">
    <h1>{{masterGame.gameName}}</h1>
    <div class="game-image-area">
      <img v-show="this.masterGame.boxartFileName" :src="boxartLink" alt="Cover Image" class="game-image">
      <font-awesome-layers v-show="!this.masterGame.boxartFileName" class="fa-8x no-game-image">
        <font-awesome-icon :icon="['far', 'square']" />
        <font-awesome-layers-text transform="shrink-14" value="No image found" />
      </font-awesome-layers>
    </div>
    <div>
      <strong>Release Date: </strong>
      <span v-if="masterGame.releaseDate">{{releaseDate(masterGame)}}</span>
      <span v-else>{{masterGame.estimatedReleaseDate}} (Estimated)</span>
    </div>
    <div>
      <label v-if="masterGame.averagedScore">This is an episodic game. We have caluclated an average score.</label>
      <div>
        <strong>Critic Score: </strong>
        {{masterGame.criticScore | score(2)}}
        <span v-if="masterGame.averagedScore">(Averaged Score)</span>
      </div>
    </div>
    <p v-if="masterGame.openCriticID">
      <a :href="openCriticLink(masterGame)" target="_blank">Open Critic Link <font-awesome-icon icon="external-link-alt" size="xs" /></a>
    </p>

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
