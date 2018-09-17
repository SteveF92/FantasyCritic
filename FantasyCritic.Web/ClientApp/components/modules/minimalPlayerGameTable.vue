<template>
  <div class="player-table">
    <table class="table table-bordered table-striped">
      <thead>
        <tr class="table-primary">
          <th scope="col" colspan="4">
            {{publisher.publisherName}}
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
        <minimalPlayerGameRow v-for="game in draftGames" :game="game"></minimalPlayerGameRow>
        <minimalBlankPlayerGameRow v-for="blankSpace in draftFiller"></minimalBlankPlayerGameRow>
        <minimalPlayerGameRow v-for="game in waiverGames" :game="game"></minimalPlayerGameRow>
        <minimalBlankPlayerGameRow v-for="blankSpace in waiverFiller"></minimalBlankPlayerGameRow>
        <minimalPlayerGameRow v-for="game in counterPicks" :game="game"></minimalPlayerGameRow>
        <minimalBlankPlayerGameRow v-for="blankSpace in counterPickFiller"></minimalBlankPlayerGameRow>
        <tr class="minimal-game-row">
          <td id="total-description">
            <span id="total-description-text">
              Total Fantasy Score
            </span>
          </td>
          <td id="total-column" class="table-success" colspan="2">{{publisher.totalFantasyScore | score}}</td>
        </tr>
      </tbody>
    </table>
  </div>
    
</template>
<script>
    import Vue from "vue";
    import MinimalPlayerGameRow from "components/modules/minimalPlayerGameRow";
    import MinimalBlankPlayerGameRow from "components/modules/minimalBlankPlayerGameRow";

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
            draftGames() {
                return _.filter(this.games, { 'counterPick': false, 'waiver': false });
            },
            counterPicks() {
                return _.filter(this.games, { 'counterPick': true });
            },
            waiverGames() {
                return _.filter(this.games, { 'waiver': true });
            },
            draftFiller() {
                var numberDrafted = this.draftGames.length;
                var openSlots = this.options.draftSlots - numberDrafted;
                return openSlots;
            },
            counterPickFiller() {
                var numberCounterPicked = this.counterPicks.length;
                var openSlots = this.options.counterPickSlots - numberCounterPicked;
                return openSlots;
            },
            waiverFiller() {
                var numberWaiverClaimed = this.waiverGames.length;
                var openSlots = this.options.waiverSlots - numberWaiverClaimed;
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
