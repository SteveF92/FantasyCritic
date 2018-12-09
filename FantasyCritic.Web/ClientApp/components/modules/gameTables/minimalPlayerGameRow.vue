<template>
  <tr class="minimal-game-row table-default" v-bind:class="{ 'table-danger': game.counterPick }">
    <td class="game-column">
      <popper trigger="click" :options="{ placement: 'top', modifiers: { offset: { offset: '0,10px' } }}">
        <div class="popper">
          <masterGamePopover :mastergameid="game.masterGameID"></masterGamePopover>
        </div>

        <span slot="reference" class="text-primary fake-link">
          {{game.gameName}}
        </span>
      </popper>

      <span v-if="!game.linked">{{game.gameName}}</span>

      <span v-if="game.counterPick" class="counter-pick-text">
        (Counter-Pick)
      </span>

      <span v-if="!game.linked" class="game-status">
        Not linked to Master Game
      </span>

      <span v-if="!game.willRelease && game.linked" class="game-status">
        <span v-show="!yearFinished">Will not Release</span>
        <span v-show="yearFinished">Did not Release</span>
      </span>
      <span v-if="game.manualCriticScore && game.linked" class="game-status">
        Manually Scored
      </span>
    </td>
    <td class="score-column">{{game.criticScore | score}}</td>
    <td class="score-column">{{game.fantasyPoints | score}}</td>
  </tr>
</template>
<script>
  import Vue from "vue";
  import moment from "moment";
  import Popper from 'vue-popperjs';
  import 'vue-popperjs/dist/css/vue-popper.css';
  import MasterGamePopover from "components/modules/masterGamePopover";

  export default {
    components: {
      'popper': Popper,
      MasterGamePopover
    },
    props: ['game', 'yearFinished'],
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
  .fake-link {
    color: blue;
    text-decoration: underline;
    cursor: pointer;
  }
  .popper {
    background: #415262;
  }
</style>
