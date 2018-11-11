<template>
  <div class="player-table" v-bind:class="{ 'publisher-is-next': publisher.nextToDraft }">
    <table class="table table-bordered table-striped">
      <thead>
        <tr class="table-primary">
          <th scope="col" colspan="4">
            <router-link :to="{ name: 'publisher', params: { publisherid: publisher.publisherID }}">{{ publisher.publisherName }}</router-link>
            <br />
            Player: {{publisher.playerName}}
          </th>
        </tr>
        <tr class="table-secondary">
          <th scope="col" class="game-column">Game</th>
          <th scope="col" class="score-column">Critic</th>
          <th scope="col" class="score-column">Points</th>
        </tr>
      </thead>
      <tbody>
        <minimalPlayerGameRow v-for="game in standardGames" :game="game"></minimalPlayerGameRow>
        <minimalBlankPlayerGameRow v-for="blankSpace in standardFiller"></minimalBlankPlayerGameRow>
        <minimalPlayerGameRow v-for="game in counterPicks" :game="game"></minimalPlayerGameRow>
        <minimalBlankPlayerGameRow v-for="blankSpace in counterPickFiller"></minimalBlankPlayerGameRow>
        <tr class="minimal-game-row">
          <td id="total-description">
            <span id="total-description-text">
              Total Fantasy Points
            </span>
          </td>
          <td id="total-column" class="table-success" colspan="2">{{publisher.totalFantasyPoints | score}}</td>
        </tr>
      </tbody>
    </table>
  </div>
    
</template>
<script>
    import Vue from "vue";
    import MinimalPlayerGameRow from "components/modules/gameTables/minimalPlayerGameRow";
    import MinimalBlankPlayerGameRow from "components/modules/gameTables/minimalBlankPlayerGameRow";

    export default {
        components: {
            MinimalPlayerGameRow,
            MinimalBlankPlayerGameRow
        },
        props: ['publisher', 'options'],
        computed: {
            games() {
                return this.publisher.games;
            },
            standardGames() {
                return _.filter(this.games, { 'counterPick': false });
            },
            counterPicks() {
                return _.filter(this.games, { 'counterPick': true });
            },
            standardFiller() {
                var numberStandardGames = this.standardGames.length;
                var openSlots = this.options.standardGameSlots - numberStandardGames;
                return openSlots;
            },
            counterPickFiller() {
                var numberCounterPicked = this.counterPicks.length;
                var openSlots = this.options.counterPickSlots - numberCounterPicked;
                return openSlots;
            }
        }
    }
</script>
<style scoped>
  .player-table {
    margin-left: 3px;
    margin-right: 7px;
    border-radius: 2px;
    border-color: #4E5D6C;
    border-width: 5px;
    border-style: solid;
    margin-bottom: 10px;
  }
  .publisher-is-next {
    border-color: #5CB85C;
  }
  .publisher-is-next table thead tr.table-primary th {
    background-color: #ED9D2B;
  }
  .player-table table {
    margin-bottom: 0px;
  }
</style>
<style>
  .player-table table thead tr th {
    height: 35px;
    padding: 5px;
  }
  .player-table table tbody tr td {
    height: 35px;
    padding: 5px;
  }
  
  .type-column {
    width: 30px;
    text-align: center;
  }
  .game-column {

  }

  .score-column {
    width: 30px;
    text-align: center;
  }

  #total-description {
    vertical-align: middle;
  }

  #total-description-text {
    float: right;
    text-align: right;
    display: table-cell;
    vertical-align: middle;
    font-weight: bold;
    font-size: 20px;
  }

  #total-column {
    text-align: center;
    font-weight: bold;
    font-size: 25px;
  }
</style>
