<template>
  <b-modal id="currentDropsForm" ref="currentDropsFormRef" title="My Pending Drop Requests" @hidden="clearData">
    <table class="table table-sm table-bordered table-striped">
      <thead>
        <tr class="bg-primary">
          <th scope="col" class="game-column">Game</th>
          <th scope="col">Cancel</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="drop in currentDrops" :key="drop.dropRequestID">
          <td>{{ drop.masterGame.gameName }}</td>
          <td class="select-cell">
            <b-button variant="danger" size="sm" @click="confirmCancelDrop(drop)">Cancel</b-button>
          </td>
        </tr>
      </tbody>
    </table>
    <b-modal id="confirm-cancel-drop-modal" @ok="cancelDrop">
      <!-- prettier-ignore -->
      <p>
        Are you sure you want to cancel your drop for
        <strong>{{ dropToCancel?.masterGame.gameName }}</strong>?
      </p>
    </b-modal>
  </b-modal>
</template>

<script>
import axios from 'axios';
import LeagueMixin from '@/mixins/leagueMixin.js';

export default {
  mixins: [LeagueMixin],
  data() {
    return {
      dropToCancel: null
    };
  },
  created() {
    this.clearData();
  },
  methods: {
    confirmCancelDrop(drop) {
      this.dropToCancel = drop;
      this.$bvModal.show('confirm-cancel-drop-modal');
    },
    cancelDrop() {
      const model = {
        publisherID: this.userPublisher.publisherID,
        dropRequestID: this.dropToCancel.dropRequestID
      };
      axios
        .post('/api/league/DeleteDropRequest', model)
        .then(() => {
          this.notifyAction('Drop Request for ' + this.dropToCancel.masterGame.gameName + ' was cancelled.');
          this.clearData();
        })
        .catch(() => {});
    },
    clearData() {
      this.dropToCancel = null;
    }
  }
};
</script>
