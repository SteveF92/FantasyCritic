<template>
  <b-modal id="currentBidsForm" ref="currentBidsFormRef" size="lg" title="My Current Bids" @hidden="clearData">
    <div class="alert alert-danger" v-show="errorInfo" role="alert">
      {{ errorInfo }}
    </div>
    <label v-show="settingPriority">Drag and drop to change order.</label>

    <b-form-checkbox v-model="settingPriority" class="priority-checkbox">
      <span class="checkbox-label">Change Priorities</span>
    </b-form-checkbox>

    <b-table-simple responsive bordered>
      <b-thead>
        <b-tr class="bg-primary">
          <b-th v-show="settingPriority"></b-th>
          <b-th class="game-column">Game</b-th>
          <b-th>Bid Amount</b-th>
          <b-th>Priority</b-th>
          <b-th>Conditional Drop</b-th>
          <b-th>Counter Pick Bid?</b-th>
          <b-th v-show="!settingPriority">Edit</b-th>
          <b-th v-show="!settingPriority">Cancel</b-th>
        </b-tr>
      </b-thead>
      <draggable v-model="desiredBidPriorities" tag="tbody" v-if="settingPriority" handle=".handle">
        <b-tr v-for="bid in desiredBidPriorities" :key="bid.bidID">
          <b-td class="handle"><font-awesome-icon icon="bars" size="lg" /></b-td>
          <b-td>{{ bid.masterGame.gameName }}</b-td>
          <b-td>{{ bid.bidAmount | money }}</b-td>
          <b-td>{{ bid.priority }}</b-td>
          <b-td v-if="bid.conditionalDropPublisherGame">{{ bid.conditionalDropPublisherGame.gameName }}</b-td>
          <b-td v-else>None</b-td>
          <b-td>{{ bid.counterPick | yesNo }}</b-td>
        </b-tr>
      </draggable>
      <b-tbody v-if="!settingPriority">
        <b-tr v-for="bid in desiredBidPriorities" :key="bid.bidID">
          <b-td>{{ bid.masterGame.gameName }}</b-td>
          <b-td>{{ bid.bidAmount | money }}</b-td>
          <b-td>{{ bid.priority }}</b-td>
          <b-td v-if="bid.conditionalDropPublisherGame">{{ bid.conditionalDropPublisherGame.gameName }}</b-td>
          <b-td v-else>None</b-td>
          <b-td>{{ bid.counterPick | yesNo }}</b-td>
          <b-td class="select-cell">
            <b-button variant="info" size="sm" v-on:click="startEditingBid(bid)">Edit</b-button>
          </b-td>
          <b-td class="select-cell">
            <b-button variant="danger" size="sm" v-on:click="cancelBid(bid)">Cancel</b-button>
          </b-td>
        </b-tr>
      </b-tbody>
    </b-table-simple>
    <div v-if="bidBeingEdited">
      <h3 for="bidBeingEdited" class="selected-game text-black">Edit Bid:</h3>
      <masterGameSummary :masterGame="bidBeingEdited.masterGame"></masterGameSummary>
      <div class="form-group">
        <label for="bidAmount" class="control-label">Bid Amount (Remaining: {{ leagueYear.userPublisher.budget | money }})</label>

        <ValidationProvider rules="required|integer" v-slot="{ errors }">
          <input v-model="bidAmount" id="bidAmount" name="bidAmount" type="number" class="form-control input" />
          <span class="text-danger">{{ errors[0] }}</span>
        </ValidationProvider>
      </div>
      <div class="form-group" v-if="!bidBeingEdited.counterPick">
        <label for="conditionalDrop" class="control-label">Conditional Drop</label>
        <b-form-select v-model="conditionalDrop">
          <option v-for="publisherGame in droppableGames" v-bind:value="publisherGame" :key="publisherGame.publisherGameID">
            {{ publisherGame.gameName }}
          </option>
        </b-form-select>
      </div>
      <b-button variant="primary" v-on:click="editBid" class="add-game-button" :disabled="isBusy">Edit Bid</b-button>
      <div v-if="editBidResult && !editBidResult.success" class="alert bid-error alert-danger">
        <h3 class="alert-heading">Error!</h3>
        <ul>
          <li v-for="error in editBidResult.errors" :key="error">{{ error }}</li>
        </ul>
      </div>
    </div>
    <div slot="modal-footer">
      <b-button variant="primary" v-if="settingPriority" v-on:click="setBidPriorityOrder()">Set Priority Order</b-button>
    </div>
  </b-modal>
</template>

<script>
import axios from 'axios';
import draggable from 'vuedraggable';
import MasterGameSummary from '@/components/masterGameSummary';
import LeagueMixin from '@/mixins/leagueMixin';

export default {
  components: {
    draggable,
    MasterGameSummary
  },
  mixins: [LeagueMixin],
  data() {
    return {
      desiredBidPriorities: [],
      settingPriority: false,
      errorInfo: null,
      bidBeingEdited: null,
      bidAmount: null,
      conditionalDrop: null,
      editBidResult: null,
      isBusy: false,
      defaultCondtionalDrop: {
        value: null,
        gameName: '<No condtional drop>'
      }
    };
  },
  computed: {
    droppableGames() {
      let list = _.filter(this.userPublisher.games, { counterPick: false });
      list.unshift(this.defaultCondtionalDrop);
      return list;
    }
  },
  mounted() {
    this.clearData();
  },
  watch: {
    currentBids(newValue, oldValue) {
      if (!oldValue || (oldValue.constructor === Array && newValue.constructor === Array && oldValue.length !== newValue.length)) {
        this.clearData();
      }
    }
  },
  methods: {
    setBidPriorityOrder() {
      let desiredBidPriorityIDs = this.desiredBidPriorities.map(function (v) {
        return v.bidID;
      });
      var model = {
        publisherID: this.userPublisher.publisherID,
        BidPriorities: desiredBidPriorityIDs
      };
      axios
        .post('/api/league/SetBidPriorities', model)
        .then(() => {
          this.$refs.currentBidsFormRef.hide();
          this.$emit('bidPriorityEdited');
        })
        .catch(() => {});
    },
    startEditingBid(bid) {
      this.bidBeingEdited = bid;
      this.bidAmount = bid.bidAmount;
      this.conditionalDrop = bid.conditionalDropPublisherGame;
    },
    editBid() {
      var request = {
        bidID: this.bidBeingEdited.bidID,
        publisherID: this.userPublisher.publisherID,
        bidAmount: this.bidAmount
      };

      if (this.conditionalDrop) {
        request.conditionalDropPublisherGameID = this.conditionalDrop.publisherGameID;
      }

      axios
        .post('/api/league/EditPickupBid', request)
        .then((response) => {
          this.editBidResult = response.data;
          if (!this.editBidResult.success) {
            return;
          }
          this.$refs.currentBidsFormRef.hide();
          this.notifyAction('Bid for ' + this.bidBeingEdited.masterGame.gameName + ' for $' + this.bidAmount + ' was made.');
          this.clearData();
        })
        .catch((response) => {
          this.errorInfo = response.response.data;
        });
    },
    cancelBid(bid) {
      var model = {
        bidID: bid.bidID,
        publisherID: this.userPublisher.publisherID
      };
      axios
        .post('/api/league/DeletePickupBid', model)
        .then(() => {
          this.notifyAction('Bid for ' + bid.masterGame.gameName + ' for $' + bid.bidAmount + ' was canceled.');
        })
        .catch((response) => {
          this.errorInfo = response.response.data;
        });
    },
    clearData() {
      this.desiredBidPriorities = this.currentBids;
      this.bidBeingEdited = null;
      this.conditionalDrop = null;
      this.bidAmount = null;
    }
  }
};
</script>
<style scoped>
.add-game-button {
  width: 100%;
}
.select-cell {
  text-align: center;
}
.priority-checkbox {
  float: right;
}
</style>
