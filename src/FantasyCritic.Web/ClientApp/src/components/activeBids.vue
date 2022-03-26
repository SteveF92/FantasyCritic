<template>
  <div>
    <div v-if="games && games.length > 0">
      <b-table :sort-by.sync="sortBy" :sort-desc.sync="sortDesc" :items="games" :fields="gamesFields" bordered striped responsive small>
        <template #cell(gameName)="data">
          <span class="game-name-column">
            <span v-if="data.item.counterPick" v-b-popover.hover.focus="counterPickText" class="badge tag-badge counter-pick-badge">CP</span>
            <masterGamePopover :master-game="data.item.masterGame"></masterGamePopover>
            <font-awesome-icon
              v-if="data.item.eligibilityErrors && data.item.eligibilityErrors.length > 0"
              v-b-popover.hover.focus="bidWillFailText(data.item)"
              class="bid-will-fail"
              color="white"
              icon="exclamation-triangle" />
          </span>
        </template>
        <template #cell(maximumReleaseDate)="data">
          {{ getReleaseDate(data.item.masterGame) }}
        </template>
      </b-table>
    </div>
    <div v-else>
      <div class="alert alert-info" role="alert">There are no games being bid upon this week.</div>
    </div>
  </div>
</template>
<script>
import moment from 'moment';
import MasterGamePopover from '@/components/masterGamePopover';
import LeagueMixin from '@/mixins/leagueMixin';

export default {
  components: {
    MasterGamePopover
  },
  mixins: [LeagueMixin],
  data() {
    return {
      sortBy: 'maximumReleaseDate',
      sortDesc: false,
      gamesFields: [
        { key: 'gameName', label: 'Name', sortable: true, thClass: 'bg-primary' },
        { key: 'maximumReleaseDate', label: 'Release Date', sortable: true, thClass: 'bg-primary' }
      ]
    };
  },
  computed: {
    counterPickText() {
      return {
        html: true,
        title: () => {
          return 'Counter Pick Bid';
        },
        content: () => {
          return 'This game is being bid on as a counter pick.';
        }
      };
    },
    games() {
      return this.leagueYear.publicBiddingGames;
    }
  },
  methods: {
    getReleaseDate(game) {
      if (game.releaseDate) {
        return moment(game.releaseDate).format('YYYY-MM-DD');
      }
      return game.estimatedReleaseDate + ' (Estimated)';
    },
    bidWillFailText(publicBid) {
      return {
        html: true,
        title: () => {
          return 'This bid will fail!';
        },
        content: () => {
          let eligibilityErrorsList = '';
          publicBid.eligibilityErrors.forEach((error) => {
            eligibilityErrorsList += `<li>${error}</li>`;
          });

          let eligibilityErrorsListElement = `<h5>Errors</h5><ul>${eligibilityErrorsList}</ul>`;
          let mainText = 'This bid will fail for the below reasons:';
          let fullText = `${mainText}<br/><br/>${eligibilityErrorsListElement}`;
          return fullText;
        }
      };
    }
  }
};
</script>
<style scoped>
.game-name-column {
  display: flex;
  align-items: center;
}

.popover-header {
  color: black;
}

.counter-pick-badge {
  background-color: #aa1e1e;
  color: white;
  margin-right: 8px;
}

.bid-will-fail {
  margin-left: 5px;
}
</style>
