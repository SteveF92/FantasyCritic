<template>
  <div>
    <b-pagination v-model="currentPage" :total-rows="rows" :per-page="perPage" aria-controls="my-table"></b-pagination>

    <b-table small bordered striped responsive :items="possibleGames" :fields="gameFields" :per-page="perPage" :current-page="currentPage">
      <template slot="gameName" slot-scope="data">
        <masterGamePopover ref="gamePopoverWrapperRef" :masterGame="data.item.masterGame" v-on:newPopoverShown="newPopoverShown"></masterGamePopover>
      </template>
      <template slot="sortableEstimatedReleaseDate" slot-scope="data">
        <div v-bind:class="{ 'text-danger': data.item.masterGame.isReleased }" class="release-date">
          <span>{{data.item.masterGame.estimatedReleaseDate}}</span>
          <span v-show="data.item.masterGame.isReleased">(Released)</span>
        </div>
      </template>
      <template slot="dateAdjustedHypeFactor" slot-scope="data">
        {{data.item.masterGame.dateAdjustedHypeFactor | score(1)}}
      </template>
      <template slot="eligibilityLevel" slot-scope="data">
        <statusBadge :possibleMasterGame="data.item"></statusBadge>
      </template>
      <template slot="select" slot-scope="data">
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
          { key: 'gameName', label: 'Name', sortable: true, thClass:'bg-primary' },
          { key: 'sortableEstimatedReleaseDate', label: 'Release Date', sortable: true, thClass: 'bg-primary' },
          { key: 'dateAdjustedHypeFactor', label: 'Hype Factor', sortable: true, thClass: ['bg-primary','lg-screen-minimum'], tdClass: 'lg-screen-minimum' },
          { key: 'eligibilityLevel', label: 'Eligibility Level', sortable: true, thClass: ['bg-primary','lg-screen-minimum'], tdClass: 'lg-screen-minimum' },
          { key: 'select', label: '', thClass: 'bg-primary' }
        ],
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
      }
    },
    methods: {
      selectGame(masterGame) {
        this.selectedMasterGame = masterGame;
        this.$emit('input', this.selectedMasterGame);
      },
      newPopoverShown(masterGame) {
        this.$refs.gamePopoverWrapperRef.forEach(function (popover) {
          if (popover.masterGame.masterGameID !== masterGame.masterGameID) {
            popover.closePopover();
          }
        });
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
