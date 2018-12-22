<template>
    <div class="player-table">
        <table class="table table-bordered table-striped">
            <thead>
                <tr class="table-primary">
                    <th scope="col">Game Name</th>
                    <th scope="col">Release Date</th>
                    <th scope="col">Critic Score</th>
                    <th scope="col">Fantasy Points</th>
                </tr>
            </thead>
            <tbody>
              <playerGameRow v-for="game in standardGames" :game="game"></playerGameRow>
              <blankPlayerGameRow v-for="blankSpace in standardFiller"></blankPlayerGameRow>
              <playerGameRow v-for="game in counterPicks" :game="game"></playerGameRow>
              <blankPlayerGameRow v-for="blankSpace in counterPickFiller"></blankPlayerGameRow>
              <tr>
                <td id="total-description">
                  <span id="total-description-text">
                    Total Fantasy Points
                  </span>
                </td>
                <td></td>
                <td id="average-critic-column">{{publisher.averageCriticScore | score(2)}} (Average)</td>
                <td id="total-column" class="success">{{publisher.totalFantasyPoints | score(2)}}</td>
              </tr>
            </tbody>
        </table>
    </div>
</template>
<script>
    import Vue from "vue";
    import PlayerGameRow from "components/modules/gameTables/playerGameRow";
    import BlankPlayerGameRow from "components/modules/gameTables/blankPlayerGameRow";

    export default {
        components: {
          PlayerGameRow,
          BlankPlayerGameRow
        },
        props: ['publisher', 'options'],
        computed: {
          standardGames() {
            return _.filter(this.publisher.games, { 'counterPick': false });
          },
          counterPicks() {
            return _.filter(this.publisher.games, { 'counterPick': true });
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
    margin-bottom: 10px;
  }
  .player-table table {
    margin-bottom: 0px;
    table-layout: fixed;
    width: 100%;
  }

  #total-description {
    vertical-align: middle;
  }

  @media only screen and (max-width: 459px) {
    #average-critic-column {
      text-align: center;
      vertical-align: middle;
      font-weight: bold;
      font-size: 14px;
    }

    #total-description-text {
      display: table-cell;
      vertical-align: middle;
      font-weight: bold;
      font-size: 14px;
    }

    #total-column {
      text-align: center;
      font-weight: bold;
      font-size: 20px;
      vertical-align: middle;
    }
  }

  @media only screen and (min-width: 460px) {
    #average-critic-column {
      text-align: center;
      vertical-align: middle;
      font-weight: bold;
      font-size: 20px;
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
      vertical-align: middle;
    }
  }
</style>
