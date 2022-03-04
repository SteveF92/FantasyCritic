<template>
  <div>
    <div v-if="masterGame" class="summary">
      <div class="game-image-area">
        <div class="full-game-image">
          <div v-if="masterGame.ggToken && masterGame.ggCoverArtFileName" class="gg-image-area">
            <img v-show="masterGame.ggCoverArtFileName" :src="ggCoverArtLink" alt="Cover Image" class="game-image">
            <a :href="ggLink" target="_blank"><strong>Image Provided by GG|<font-awesome-icon icon="external-link-alt" /></strong></a>
          </div>
          <font-awesome-layers v-show="!masterGame.ggCoverArtFileName" class="fa-8x no-game-image">
            <font-awesome-icon :icon="['far', 'square']" />
            <font-awesome-layers-text transform="shrink-14" value="No image found" />
          </font-awesome-layers>
        </div>
        <div class="minimal-game-image">
          <div v-if="masterGame.ggToken && masterGame.ggCoverArtFileName" class="gg-image-area">
            <img v-show="masterGame.ggCoverArtFileName" :src="smallGGCoverArtLink" alt="Cover Image" class="game-image">
            <a :href="ggLink" target="_blank"><strong>Image Provided by GG|<font-awesome-icon icon="external-link-alt" /></strong></a>
          </div>
          <font-awesome-layers v-show="!masterGame.ggCoverArtFileName" class="fa-4x no-game-image">
            <font-awesome-icon :icon="['far', 'square']" />
            <font-awesome-layers-text transform="shrink-14" value="No image found" />
          </font-awesome-layers>
        </div>
      </div>

      <div class="game-description">
        <h4 class="game-name">{{masterGame.gameName}}</h4>

        <div v-if="masterGame.tags && masterGame.tags.length > 0" class="long-tag-list">
          <strong>Tags: </strong>
          <span v-for="(tag, index) in masterGame.tags">
            <masterGameTagBadge :tagName="masterGame.tags[index]"></masterGameTagBadge>
          </span>
        </div>

        <div>
          <strong>Release Date: </strong>
          <span v-if="masterGame.releaseDate">{{releaseDate}}</span>
          <span v-else>{{masterGame.estimatedReleaseDate}} (Estimated)</span>
        </div>
        <div>
          <strong>Percent Published: </strong>
          {{masterGame.eligiblePercentStandardGame | percent(1)}}
        </div>
        <div>
          <strong>Percent Counterpicked: </strong>
          {{masterGame.adjustedPercentCounterPick | percent(1)}}
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
          <a :href="openCriticLink" target="_blank"><strong>OpenCritic Link <font-awesome-icon icon="external-link-alt" /></strong></a>
        </div>
        <div v-if="masterGame.ggToken && !masterGame.ggCoverArtFileName">
          <a :href="ggLink" target="_blank"><strong>GG| Link <font-awesome-icon icon="external-link-alt" /></strong></a>
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
</template>

<script>
import Vue from 'vue';
import axios from 'axios';
import moment from 'moment';
import MasterGameTagBadge from '@/components/modules/masterGameTagBadge';

export default {
  data() {
    return {
      error: ''
    };
  },
  props: ['masterGame'],
  components: {
    MasterGameTagBadge
  },
  computed: {
    ggCoverArtLink() {
      if (this.masterGame.ggCoverArtFileName) {
        return `https://ggapp.imgix.net/media/games/${this.masterGame.ggToken}/${this.masterGame.ggCoverArtFileName}?w=190&dpr=1&fit=crop&auto=compress&q=95`;
      }
      return null;
    },
    smallGGCoverArtLink() {
      if (this.masterGame.ggCoverArtFileName) {
        return `https://ggapp.imgix.net/media/games/${this.masterGame.ggToken}/${this.masterGame.ggCoverArtFileName}?w=100&dpr=1&fit=crop&auto=compress&q=95`;
      }
      return null;
    },
    isAdmin() {
      return this.$store.getters.isAdmin;
    },
    releaseDate() {
      return moment(this.masterGame.releaseDate).format('MMMM Do, YYYY');
    },
    openCriticLink() {
      return `https://opencritic.com/game/${this.masterGame.openCriticID}/a`;
    },
    ggLink() {
      return `https://ggapp.io/games/${this.masterGame.ggToken}`;
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

  .gg-image-area {
    display: flex;
    flex-direction: column;
    align-items: center;
  }

  .game-image {
    display: block;
    margin: auto;
    border-radius: 5%;
  }

  .no-game-image {
  }

  .game-description {
    margin-left: 5px;
    margin-right: 20px;
  }

  .game-name {
    text-align: center;
  }

  @media only screen and (min-width: 500px) {
    .minimal-game-image {
      display: none;
    }
  }

  @media only screen and (max-width: 501px) {
    .full-game-image {
      display: none;
    }
  }

</style>
