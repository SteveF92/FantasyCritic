<template>
  <b-modal id="currentBidsForm" ref="currentBidsFormRef" size="lg" title="My Current Bids" @hidden="clearData">
    <div class="alert alert-danger" v-show="errorInfo" role="alert">
      {{ errorInfo }}
    </div>
    <label v-show="settingPriority">Drag and drop to change order.</label>

    <b-form-checkbox v-model="settingPriority" class="priority-checkbox">
      <span class="checkbox-label">Change Priorities</span>
    </b-form-checkbox>

    <table class="table table-sm table-bordered table-striped">
      <thead>
        <tr class="bg-primary">
          <th scope="col" v-show="settingPriority"></th>
          <th scope="col" class="game-column">Game</th>
          <th scope="col">Bid Amount</th>
          <th scope="col">Priority</th>
          <th scope="col">Conditional Drop</th>
          <th scope="col">Counter Pick Bid?</th>
          <th scope="col" v-show="!settingPriority">Edit</th>
          <th scope="col" v-show="!settingPriority">Cancel</th>
        </tr>
      </thead>
      <draggable v-model="desiredBidPriorities" tag="tbody" v-if="settingPriority" handle=".handle">
        <tr v-for="bid in desiredBidPriorities" :key="bid.priority">
          <td scope="row" class="handle"><font-awesome-icon icon="bars" size="lg" /></td>
          <td>{{ bid.masterGame.gameName }}</td>
          <td>{{ bid.bidAmount | money }}</td>
          <td>{{ bid.priority }}</td>
          <td v-if="bid.conditionalDropPublisherGame">{{ bid.conditionalDropPublisherGame.gameName }}</td>
          <td v-else>None</td>
          <td>{{ bid.counterPick | yesNo }}</td>
        </tr>
      </draggable>
      <tbody v-if="!settingPriority">
        <tr v-for="bid in desiredBidPriorities">
          <td>{{ bid.masterGame.gameName }}</td>
          <td>{{ bid.bidAmount | money }}</td>
          <td>{{ bid.priority }}</td>
          <td v-if="bid.conditionalDropPublisherGame">{{ bid.conditionalDropPublisherGame.gameName }}</td>
          <td v-else>None</td>
          <td>{{ bid.counterPick | yesNo }}</td>
          <td class="select-cell">
            <b-button variant="info" size="sm" v-on:click="startEditingBid(bid)">Edit</b-button>
          </td>
          <td class="select-cell">
            <b-button variant="danger" size="sm" v-on:click="cancelBid(bid)">Cancel</b-button>
          </td>
        </tr>
      </tbody>
    </table>
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
          <option v-for="publisherGame in droppableGames" v-bind:value="publisherGame">
            {{ publisherGame.gameName }}
          </option>
        </b-form-select>
      </div>
      <b-button variant="primary" v-on:click="editBid" class="add-game-button" :disabled="isBusy">Edit Bid</b-button>
      <div v-if="editBidResult && !editBidResult.success" class="alert bid-error alert-danger">
        <h3 class="alert-heading">Error!</h3>
        <ul>
          <li v-for="error in editBidResult.errors">{{ error }}</li>
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

export default {
  components: {
    draggable,
    MasterGameSummary
  },
  props: ['leagueYear', 'publisher', 'currentBids'],
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
      let list = _.filter(this.publisher.games, { counterPick: false });
      list.unshift(this.defaultCondtionalDrop);
      return list;
    }
  },
  methods: {
    setBidPriorityOrder() {
      let desiredBidPriorityIDs = this.desiredBidPriorities.map(function (v) {
        return v.bidID;
      });
      var model = {
        publisherID: this.publisher.publisherID,
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
          var bidInfo = {
            gameName: this.bidBeingEdited.masterGame.gameName,
            bidAmount: this.bidAmount
          };
          this.$emit('bidEdited', bidInfo);
          this.clearData();
        })
        .catch((response) => {
          this.errorInfo = response.response.data;
        });
    },
    cancelBid(bid) {
      var model = {
        bidID: bid.bidID
      };
      axios
        .post('/api/league/DeletePickupBid', model)
        .then(() => {
          var bidInfo = {
            gameName: bid.masterGame.gameName,
            bidAmount: bid.bidAmount
          };
          this.$emit('bidCanceled', bidInfo);
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
