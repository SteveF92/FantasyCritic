<template>
  <div>
    <b-pagination v-model="currentPage" :total-rows="rows" :per-page="perPage" aria-controls="my-table"></b-pagination>

    <b-table :sort-by.sync="sortBy"
             :sort-desc.sync="sortDesc"
             :items="gameRows"
             :fields="gameFields"
             :per-page="perPage"
             :current-page="currentPage"
             bordered small responsive striped>
      <template v-slot:cell(masterGame.gameName)="data">
        <masterGamePopover ref="gamePopoverWrapperRef" :masterGame="data.item.masterGame"></masterGamePopover>
      </template>
      <template v-slot:cell(masterGame.sortableEstimatedReleaseDate)="data">
        <div v-bind:class="{ 'text-danger': data.item.masterGame.isReleased }" class="release-date">
          <span>{{data.item.masterGame.estimatedReleaseDate}}</span>
          <span v-show="data.item.masterGame.isReleased">(Released)</span>
        </div>
      </template>
      <template v-slot:cell(masterGame.dateAdjustedHypeFactor)="data">
        {{data.item.masterGame.dateAdjustedHypeFactor | score(1)}}
      </template>
      <template v-slot:cell(masterGame.eligibilityLevel)="data">
        <statusBadge :possibleMasterGame="data.item"></statusBadge>
      </template>
      <template v-slot:cell(select)="data">
        <b-button size="sm" variant="info" v-on:click="selectGame(data.item.masterGame)">Select</b-button>
      </template>
    </b-table>
  </div>
</template>
<script>
  import StatusBadge from "components/modules/statusBadge";
  import MasterGamePopover from "components/modules/masterGamePopover";

  export default {
    data() {
      return {
        selectedMasterGame: null,
        lastPopoverShown: null,
        perPage: 10,
        currentPage: 1,
        gameFields: [
          { key: 'masterGame.gameName', label: 'Name', sortable: true, thClass:'bg-primary' },
          { key: 'masterGame.sortableEstimatedReleaseDate', label: 'Release Date', sortable: true, thClass: 'bg-primary' },
          { key: 'masterGame.dateAdjustedHypeFactor', label: 'Hype Factor', sortable: true, thClass: ['bg-primary','lg-screen-minimum'], tdClass: 'lg-screen-minimum' },
          { key: 'masterGame.eligibilityLevel', label: 'Eligibility Level', thClass: ['bg-primary','lg-screen-minimum'], tdClass: 'lg-screen-minimum' },
          { key: 'select', label: '', thClass: 'bg-primary' }
        ],
        sortBy: 'dateAdjustedHypeFactor',
        sortDesc: true
      }
    },
    components: {
      StatusBadge,
      MasterGamePopover
    },
    props: ['possibleGames', 'value', 'maximumEligibilityLevel'],
    computed: {
      rows() {
        return this.possibleGames.length;
      },
      gameRows() {
        let gameRows = this.possibleGames;
        if (!gameRows) {
            return [];
        }
        return gameRows;
      }
    },
    methods: {
      selectGame(masterGame) {
        this.selectedMasterGame = masterGame;
        this.$emit('input', this.selectedMasterGame);
      }
    }
  }
</script>
<style scoped>
  .fake-link {
    text-decoration: underline;
    cursor: pointer;
  }

  .popper {
    background: #415262;
  }

  .release-date{
    font-weight: bold;
  }

  .select-cell {
    text-align: center;
  }

  @media only screen and (max-width: 450px) {
    .no-mobile {
      display: none;
    }
  }
</style>
