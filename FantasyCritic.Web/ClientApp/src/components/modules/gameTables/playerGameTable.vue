<template>
  <div class="table-responsive">
    <table class="table table-bordered table-striped table-sm player-full-table">
      <thead>
        <tr class="bg-primary">
          <th scope="col">Game Name</th>
          <th scope="col">Release Date</th>
          <th scope="col">Date Acquired</th>
          <th scope="col">Critic Score</th>
          <th scope="col">Fantasy Points</th>
        </tr>
      </thead>
      <tbody>
        <playerGameSlotRow v-for="gameSlot in storedGameSlots"
                           :gameSlot="gameSlot" :supportedYear="leagueYear.supportedYear" :hasSpecialSlots="leagueYear.hasSpecialSlots"
                           v-bind:key="gameSlot.overallSlotNumber"></playerGameSlotRow>
        <tr>
          <td id="total-description">
            <span id="total-description-text">
              Total Fantasy Points
            </span>
          </td>
          <td></td>
          <td></td>
          <td id="average-critic-column">{{publisher.averageCriticScore | score(2)}} (Average)</td>
          <td id="total-column" class="success">{{publisher.totalFantasyPoints | score(2)}}</td>
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
    storedGameSlots() {
      return this.$store.getters.gameSlots;
    }
  }
};
</script>
<style scoped>
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
<style>
  .player-full-table tbody tr td {
    border: 1px solid white;
  }
</style>
