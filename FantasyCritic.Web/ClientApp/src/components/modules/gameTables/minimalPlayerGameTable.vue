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
      <tbody>
        <playerGameSlotRow v-for="gameSlot in publisher.gameSlots" :minimal="true"
                           :gameSlot="gameSlot" :yearFinished="leagueYear.supportedYear.finished" 
                           :hasSpecialSlots="leagueYear.hasSpecialSlots" :userIsPublisher="userIsPublisher"
                           v-bind:key="gameSlot.overallSlotNumber"></playerGameSlotRow>
          <tr class="minimal-game-row">
            <td id="total-description">
              <span id="total-description-text" v-if="!advancedProjections">
                Total Fantasy Points
              </span>
              <span id="total-description-text" v-else>
                Projected Fantasy Points
              </span>
            </td>
            <template v-if="!advancedProjections">
              <td id="total-column" class="bg-success" colspan="2">{{publisher.totalFantasyPoints | score}}</td>
            </template>
            <template v-else>
              <td id="total-column" class="bg-info" colspan="2"><em>~{{publisher.totalProjectedPoints | score}}</em></td>
            </template>
          </tr>
      </tbody>
    </table>
  </div>

</template>
<script>
import PlayerGameSlotRow from '@/components/modules/gameTables/playerGameSlotRow';

export default {
  components: {
    PlayerGameSlotRow
  },
  props: ['publisher', 'leagueYear'],
  computed: {
    advancedProjections() {
      return this.$store.getters.advancedProjections;
    },
    userIsPublisher() {
      return this.$store.getters.userInfo && this.publisher.userID === this.$store.getters.userInfo.userID;
    }
  }
};
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
    margin-bottom: 0;
  }

  .publisher-player-names {
    margin: 15px;
    margin-bottom: 0;
    display: flex;
    justify-content: space-between;
    flex-wrap: wrap;
  }

  .publisher-name {
    font-weight: bold;
    text-transform: uppercase;
    font-size: 1.1em;
    color: #D6993A;
    word-break: break-word;
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
    font-weight: bold;
    height: 20px;
    line-height: 20px;
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
