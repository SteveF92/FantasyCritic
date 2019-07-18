<template>
  <div class="player-summary bg-secondary" v-bind:class="{ 'publisher-is-next': publisher.nextToDraft }">
    <div class="publisher-player-names">
      <div class="publisher-name">
        <router-link :to="{ name: 'publisher', params: { publisherid: publisher.publisherID }}">
          {{ publisher.publisherName }}
          <font-awesome-icon icon="info-circle" />
        </router-link>

      </div>
      <div class="player-name">
        Player: {{publisher.playerName}}
      </div>
    </div>
    <table class="table table-striped">
      <thead>
        <tr class="bg-secondary">
          <th scope="col" class="game-column">Game</th>
          <th scope="col" class="score-column">Critic</th>
          <th scope="col" class="score-column">Points</th>
        </tr>
      </thead>
      <tbody v-if="tableIsValid">
          <minimalPlayerGameRow v-for="game in standardGames" :game="game" :yearFinished="yearFinished"></minimalPlayerGameRow>
          <minimalBlankPlayerGameRow v-for="blankSpace in standardFiller"></minimalBlankPlayerGameRow>
          <minimalPlayerGameRow v-for="game in counterPicks" :game="game" :yearFinished="yearFinished"></minimalPlayerGameRow>
          <minimalBlankPlayerGameRow v-for="blankSpace in counterPickFiller"></minimalBlankPlayerGameRow>
          <tr class="minimal-game-row">
            <td id="total-description">
              <span id="total-description-text">
                Total Fantasy Points
              </span>
            </td>
            <td id="total-column" class="success" colspan="2">{{publisher.totalFantasyPoints | score}}</td>
          </tr>
      </tbody>
      <tbody v-if="!tableIsValid">
        <tr>
          <td scope="col" colspan="4">
            <div class="alert alert-danger">
              Something has gone wrong with this publisher! There are probably duplicated games that the league manager can remove using the "Remove Publisher Game" action. If that doesn't work,
              <router-link :to="{ name: 'contact'}">contact</router-link> me.
            </div>
          </td>
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
        props: ['publisher', 'options', 'yearFinished'],
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
            },
            tableIsValid() {
              return (this.standardGames.length <= this.options.standardGameSlots) && (this.counterPicks.length <= this.options.counterPickSlots);
            }
        }
    }
</script>
<style scoped>
  .player-summary {
    margin-bottom: 10px;
    padding: 5px;
    border: solid;
    border-width: 1px;
    padding: 0;
  }

  .publisher-is-next {
    border-color: #45621d;
    border-width: 5px;
    border-style: solid;
    padding: 0;
  }

  .publisher-is-next table thead tr.table-primary th {
    background-color: #ED9D2B;
  }

  .player-summary table {
    margin-bottom: 0px;
  }

  .publisher-player-names {
    margin: 15px;
    margin-bottom: 0;
  }

  .publisher-name {
    font-weight: bold;
    text-transform: uppercase;
    font-size: 1.1em;
    color: #D6993A;
    word-wrap: break-word;
  }

  .player-name {
    color: #D6993A;
    font-weight: bold;
  }

</style>
<style>
  .player-summary table {
    border-collapse: collapse;
    border-style: hidden;
  }

  .player-summary table td {
    border: 1px solid white;
  }

  .player-summary table thead tr th {
    height: 35px;
    padding: 5px;
  }

  .player-summary table tbody tr td {
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
