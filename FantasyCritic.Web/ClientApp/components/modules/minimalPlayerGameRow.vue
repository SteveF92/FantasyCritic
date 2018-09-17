<template>
  <tr class="minimal-game-row table-default" v-bind:class="{ 'table-danger': game.counterPick }">
    <td class="game-column">
      <span>
        {{game.gameName}}
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
    <td class="score-column">{{game.criticScore | score}}</td>
    <td class="score-column">{{game.fantasyScore | score}}</td>
  </tr>
</template>
<script>
    import Vue from "vue";
    import moment from "moment";

    export default {
        props: ['game'],
        computed: {
            releaseDate() {
                return moment(this.game.releaseDate).format('MMMM Do, YYYY');
            }
        }
    }
</script>
<style scoped>
  .minimal-game-row td {
    font-size: 10pt;
  }
  .counter-pick-text {
    color: #B1B1B1;
    font-style: italic;
  }
  .game-status {
    float: right;
    color: #B1B1B1;
    font-style: italic;
  }
</style>
