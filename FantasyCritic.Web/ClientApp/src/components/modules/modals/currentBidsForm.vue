<template>
  <b-modal id="currentBidsForm" ref="currentBidsFormRef" title="My Current Bids" @hidden="clearData">
    <label v-show="settingPriority">
      Drag and drop to change order.
    </label>

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
          <th scope="col" v-show="!settingPriority">Cancel</th>
        </tr>
      </thead>
      <draggable v-model="desiredBidPriorities" tag="tbody" v-if="settingPriority" handle=".handle">
        <tr v-for="bid in desiredBidPriorities" :key="bid.priority">
          <td scope="row" handle=".handle"><font-awesome-icon icon="bars" size="lg" /></td>
          <td>{{bid.masterGame.gameName}}</td>
          <td>{{bid.bidAmount | money}}</td>
          <td>{{bid.priority}}</td>
        </tr>
      </draggable>
      <tbody v-if="!settingPriority">
        <tr v-for="bid in desiredBidPriorities">
          <td>{{bid.masterGame.gameName}}</td>
          <td>{{bid.bidAmount | money}}</td>
          <td>{{bid.priority}}</td>
          <td class="select-cell">
            <b-button variant="danger" size="sm" v-on:click="cancelBid(bid)">Cancel</b-button>
          </td>
        </tr>
      </tbody>
    </table>
    <div slot="modal-footer">
      <b-button variant="primary" v-if="settingPriority" v-on:click="setBidPriorityOrder()">Set Priority Order</b-button>
    </div>
  </b-modal>
</template>

<script>
import Vue from 'vue';
import axios from 'axios';
import draggable from 'vuedraggable';

export default {
  components: {
    draggable,
  },
  props: ['publisher','currentBids'],
  data() {
    return {
      desiredBidPriorities: [],
      settingPriority: false
    };
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
        .then(response => {
          this.$refs.currentBidsFormRef.hide();
          this.$emit('bidPriorityEdited');
        })
        .catch(response => {

        });
    },
    cancelBid(bid) {
      var model = {
        bidID: bid.bidID
      };
      axios
        .post('/api/league/DeletePickupBid', model)
        .then(response => {
          var bidInfo = {
            gameName: bid.masterGame.gameName,
            bidAmount: bid.bidAmount
          };
          this.$emit('bidCanceled', bidInfo);
        })
        .catch(response => {

        });
    },
    clearData() {
      this.desiredBidPriorities = this.currentBids;
    }
  },
  mounted() {
    this.clearData();
  },
  watch: {
    currentBids(newValue, oldValue) {
      if (!oldValue || (oldValue.constructor === Array && newValue.constructor === Array &&
          oldValue.length !== newValue.length)) {
        this.clearData();
      }
    }
  }
};
</script>
<style scoped>
  .select-cell {
    text-align: center;
  }
  .priority-checkbox{
    float: right;
  }
</style>
