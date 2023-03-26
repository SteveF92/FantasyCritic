<template>
  <b-modal id="managerEditPublishersForm" ref="managerEditPublishersFormRef" size="lg" title="Edit Publishers" hide-footer @hidden="clearData">
    <div class="alert alert-warning">Warning! This feature is intended to fix mistakes and other exceptional circumstances. In general, managers should not be editing Publisher details.</div>
    <div class="form-horizontal">
      <div class="form-group">
        <label for="editPublisher" class="control-label">Publisher to Edit</label>
        <b-form-select v-model="editPublisher" @change="selectPublisher(editPublisher)">
          <option v-for="publisher in publishers" :key="publisher.publisherID" :value="publisher">
            {{ publisher.publisherName }}
          </option>
        </b-form-select>
      </div>
      <div v-if="editPublisher">
        <div class="form-group">
          <label for="newPublisherName" class="control-label">Publisher Name</label>
          <input id="newPublisherName" v-model="newPublisherName" name="newPublisherName" type="text" class="form-control input" />
        </div>
        <div class="form-group">
          <label for="newBudget" class="control-label">Budget</label>
          <input id="newBudget" v-model="newBudget" name="newBudget" type="text" class="form-control input" />
        </div>
        <div class="form-group">
          <label for="newFreeGamesDropped" class="control-label">"Any Unreleased" Games Dropped</label>
          <input id="newFreeGamesDropped" v-model="newFreeGamesDropped" name="newFreeGamesDropped" type="text" class="form-control input" />
        </div>
        <div class="form-group">
          <label for="newWillNotReleaseGamesDropped" class="control-label">Will not Release Games Dropped</label>
          <input id="newWillNotReleaseGamesDropped" v-model="newWillNotReleaseGamesDropped" name="newWillNotReleaseGamesDropped" type="text" class="form-control input" />
        </div>
        <div class="form-group">
          <label for="newWillReleaseGamesDropped" class="control-label">Will Release Games Dropped</label>
          <input id="newWillReleaseGamesDropped" v-model="newWillReleaseGamesDropped" name="newWillReleaseGamesDropped" type="text" class="form-control input" />
        </div>
        <div class="form-group">
          <label for="newSuperDropsAvailable" class="control-label">Super Drops Available</label>
          <input id="newSuperDropsAvailable" v-model="newSuperDropsAvailable" name="newSuperDropsAvailable" type="text" class="form-control input" />
        </div>

        <div>
          <input type="submit" class="btn btn-primary modal-submit-button" value="Edit Publisher" @click="makeEditRequest" />
        </div>
      </div>
      <br />
      <div v-if="errorInfo" class="alert alert-danger">
        <h3 class="alert-heading">Error!</h3>
        <p class="text-white">{{ errorInfo }}</p>
      </div>
    </div>
  </b-modal>
</template>

<script>
import axios from 'axios';
import LeagueMixin from '@/mixins/leagueMixin.js';

export default {
  mixins: [LeagueMixin],
  data() {
    return {
      editPublisher: null,
      newPublisherName: null,
      newBudget: null,
      newFreeGamesDropped: null,
      newWillNotReleaseGamesDropped: null,
      newWillReleaseGamesDropped: null,
      newSuperDropsAvailable: null,
      errorInfo: null
    };
  },
  methods: {
    selectPublisher(selectedPublisher) {
      this.newPublisherName = selectedPublisher.publisherName;
      this.newBudget = selectedPublisher.budget;
      this.newFreeGamesDropped = selectedPublisher.freeGamesDropped;
      this.newWillNotReleaseGamesDropped = selectedPublisher.willNotReleaseGamesDropped;
      this.newWillReleaseGamesDropped = selectedPublisher.willReleaseGamesDropped;
      this.newSuperDropsAvailable = selectedPublisher.superDropsAvailable;
    },
    makeEditRequest() {
      var model = {
        publisherID: this.editPublisher.publisherID,
        publisherName: this.newPublisherName,
        budget: this.newBudget,
        freeGamesDropped: this.newFreeGamesDropped,
        willNotReleaseGamesDropped: this.newWillNotReleaseGamesDropped,
        willReleaseGamesDropped: this.newWillReleaseGamesDropped,
        superDropsAvailable: this.newSuperDropsAvailable
      };
      axios
        .post('/api/leagueManager/EditPublisher', model)
        .then(() => {
          this.$refs.managerEditPublishersFormRef.hide();
          this.notifyAction('Publisher has been edited.');
        })
        .catch((response) => {
          this.errorInfo = response.response.data;
        });
    },
    clearData() {
      this.editPublisher = null;
    }
  }
};
</script>
