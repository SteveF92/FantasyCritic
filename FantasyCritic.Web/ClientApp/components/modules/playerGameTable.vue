<template>
    <div class="player-table">
        <table class="table table-bordered table-striped">
            <thead>
                <tr>
                    <th scope="col">Game Name</th>
                    <th scope="col">Release Date</th>
                    <th scope="col">Critic Score</th>
                    <th scope="col">Fantasy Score</th>
                </tr>
            </thead>
            <tbody>
              <playerGameRow v-for="game in draftGames" :game="game"></playerGameRow>
              <blankPlayerGameRow v-for="blankSpace in draftFiller"></blankPlayerGameRow>
              <playerGameRow v-for="game in waiverGames" :game="game"></playerGameRow>
              <blankPlayerGameRow v-for="blankSpace in waiverFiller"></blankPlayerGameRow>
              <playerGameRow v-for="game in counterPicks" :game="game"></playerGameRow>
              <blankPlayerGameRow v-for="blankSpace in counterPickFiller"></blankPlayerGameRow>
              <tr>
                <td id="total-description">
                  <span id="total-description-text">
                    Total Fantasy Score
                  </span>
                </td>
                <td></td>
                <td id="average-critic-column">{{publisher.averageCriticScore | score(2)}} (Average)</td>
                <td id="total-column" class="table-success">{{publisher.totalFantasyScore | score(2)}}</td>
              </tr>
            </tbody>
        </table>
    </div>
</template>
<script>
    import Vue from "vue";
    import PlayerGameRow from "components/modules/playerGameRow";
    import BlankPlayerGameRow from "components/modules/blankPlayerGameRow";

    export default {
        components: {
          PlayerGameRow,
          BlankPlayerGameRow
        },
        props: ['publisher', 'options'],
        computed: {
          draftGames() {
            return _.filter(this.publisher.games, { 'counterPick': false, 'waiver': false });
          },
          counterPicks() {
            return _.filter(this.publisher.games, { 'counterPick': true });
          },
          waiverGames() {
            return _.filter(this.publisher.games, { 'waiver': true });
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

  #average-critic-column {
    text-align: center;
    vertical-align: middle;
    font-weight: bold;
    font-size: 20px;
  }

  #total-description {
    vertical-align: middle;
  }

  #total-description-text {
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
