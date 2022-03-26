<template>
  <b-modal id="currentDropsForm" ref="currentDropsFormRef" title="My Pending Drop Requests">
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
            <b-button variant="danger" size="sm" @click="cancelDrop(drop)">Cancel</b-button>
          </td>
        </tr>
      </tbody>
    </table>
  </b-modal>
</template>

<script>
import axios from 'axios';
import LeagueMixin from '@/mixins/leagueMixin';

export default {
  mixins: [LeagueMixin],
  methods: {
    cancelDrop(dropRequest) {
      var model = {
        publisherID: this.userPublisher.publisherID,
        dropRequestID: dropRequest.dropRequestID
      };
      axios
        .post('/api/league/DeleteDropRequest', model)
        .then(() => {
          this.notifyAction('Drop Request for ' + dropRequest.masterGame.gameName + ' was cancelled.');
        })
        .catch(() => {});
    }
  }
};
</script>
