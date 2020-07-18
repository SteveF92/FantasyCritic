<template>
  <tr v-bind:class="{ 'table-danger': game.counterPick }">
    <td>
      <span class="master-game-popover">
        <masterGamePopover v-if="game.linked" :masterGame="game.masterGame"></masterGamePopover>
        <span v-if="!game.linked">{{game.gameName}}</span>
      </span>
      

      <span v-if="game.counterPick" class="counter-pick-text">
        (Counter-Pick)
      </span>

      <span v-if="!game.linked" class="game-status">
        Not linked to Master Game
      </span>

      <span v-if="!game.willRelease && game.linked" class="game-status">
        Will not Release
      </span>
    </td>
    <td v-if="game.releaseDate">{{releaseDate}}</td>
    <td v-else>{{game.estimatedReleaseDate}} (Estimated)</td>
    <td>{{game.criticScore | score(2)}}</td>
    <td>{{game.fantasyPoints | score(2)}}</td>
  </tr>
</template>
<script>
  import Vue from "vue";
  import moment from "moment";
  import MasterGamePopover from "components/modules/masterGamePopover";

  export default {
    components: {
      MasterGamePopover
    },
    props: ['game'],
    computed: {
      releaseDate() {
        return moment(this.game.releaseDate).format('MMMM Do, YYYY');
      }
    }
  }
</script>
<style scoped>
  tr {
    height: 40px;
  }
  .counter-pick-text {
    color: #B1B1B1;
    font-style: italic;
    margin-left: 3px;
  }

  .game-status {
    float: right;
    color: #B1B1B1;
    font-style: italic;
  }
  .master-game-popover {
    float: left;
  }
</style>
