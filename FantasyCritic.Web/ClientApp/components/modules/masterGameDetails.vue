<template>
  <div v-if="masterGame">
    <ul>
      <li>
        <strong>Release Date: </strong>
        <span v-if="masterGame.releaseDate">{{releaseDate(masterGame)}}</span>
        <span v-else>{{masterGame.estimatedReleaseDate}} (Estimated)</span>
      </li>
      <li>
        <label v-if="masterGame.averagedScore">This is an episodic game. We have caluclated an average score.</label>
        <div>
          <strong>Critic Score: </strong>
          {{masterGame.criticScore | score(2)}}
          <span v-if="masterGame.averagedScore">(Averaged Score)</span>
        </div>
      </li>
      <li v-if="masterGame.openCriticID">
        <a :href="openCriticLink(masterGame)" target="_blank">Open Critic Link <font-awesome-icon icon="external-link-alt" size="xs" /></a>
      </li>
    </ul>
  </div>
</template>

<script>
  import moment from "moment";

  export default {
    props: ['masterGame'],
    methods: {
      releaseDate(game) {
        return moment(game.releaseDate).format('MMMM Do, YYYY');
      },
      openCriticLink(game) {
        return "https://opencritic.com/game/" + game.openCriticID + "/a";
      }
    }
  }
</script>
