<template>
  <b-modal id="gameQueueForm" ref="gameQueueFormRef" title="My Watchlist" @hidden="clearData">
    <label>
      Drag and drop to change order.
    </label>
    <table class="table table-sm table-responsive-sm table-bordered table-striped">
      <thead>
        <tr class="bg-primary">
          <th scope="col"></th>
          <th scope="col" class="game-column">Game</th>
          <th scope="col">Bid Amount</th>
          <th scope="col">Priority</th>
          <th scope="col">Cancel</th>
        </tr>
      </thead>
      <draggable v-model="desiredBidPriorities" tag="tbody">
        <tr v-for="bid in desiredBidPriorities" :key="bid.priority">
          <td scope="row"><font-awesome-icon icon="bars" size="lg" /></td>
          <td>{{bid.masterGame.gameName}}</td>
          <td>{{bid.bidAmount | money}}</td>
          <td>{{bid.priority}}</td>
          <td class="select-cell">
            <b-button variant="danger" size="sm" v-on:click="cancelBid(bid)">Cancel</b-button>
          </td>
        </tr>
      </draggable>
    </table>
    <div slot="modal-footer">
      <input type="submit" class="btn btn-primary" value="Set Priority Order" v-on:click="setBidPriorityOrder" />
    </div>
  </b-modal>
</template>

<script>
  import Vue from "vue";
  import axios from "axios";
  import draggable from 'vuedraggable';

  export default {
    components: {
      draggable,
    },
    props: ['publisher', 'queuedGames'],
    data() {
      return {
        desiredBidPriorities: []
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
  }
</script>
<style scoped>
  .select-cell {
    text-align: center;
  }
</style>
