<template>
  <b-modal id="managerEditPublishersForm" ref="managerEditPublishersFormRef" size="lg" title="Edit Publishers" hide-footer @hidden="clearData">
    <div class="alert alert-warning">Warning! This feature is intended to fix mistakes and other exceptional circumstances. In general, managers should not be editing Publisher details.</div>
    <div class="form-horizontal">
      <div class="form-group">
        <label for="editPublisher" class="control-label">Publisher to Edit</label>
        <b-form-select v-model="editPublisher" v-on:change="selectPublisher(editPublisher)">
          <option v-for="publisher in publishers" v-bind:value="publisher">
            {{ publisher.publisherName }}
          </option>
        </b-form-select>
      </div>
      <div v-if="editPublisher">
        <div class="form-group">
          <label for="newPublisherName" class="control-label">Publisher Name</label>
          <input v-model="newPublisherName" id="newPublisherName" name="newPublisherName" type="text" class="form-control input" />
        </div>
        <div class="form-group">
          <label for="newBudget" class="control-label">Budget</label>
          <input v-model="newBudget" id="newBudget" name="newBudget" type="text" class="form-control input" />
        </div>
        <div class="form-group">
          <label for="newFreeGamesDropped" class="control-label">"Any Unreleased" Games Dropped</label>
          <input v-model="newFreeGamesDropped" id="newFreeGamesDropped" name="newFreeGamesDropped" type="text" class="form-control input" />
        </div>
        <div class="form-group">
          <label for="newWillNotReleaseGamesDropped" class="control-label">Will not Release Games Dropped</label>
          <input v-model="newWillNotReleaseGamesDropped" id="newWillNotReleaseGamesDropped" name="newWillNotReleaseGamesDropped" type="text" class="form-control input" />
        </div>
        <div class="form-group">
          <label for="newWillReleaseGamesDropped" class="control-label">Will Release Games Dropped</label>
          <input v-model="newWillReleaseGamesDropped" id="newWillReleaseGamesDropped" name="newWillReleaseGamesDropped" type="text" class="form-control input" />
        </div>

        <div>
          <input type="submit" class="btn btn-primary submit-button" value="Edit Publisher" v-on:click="makeEditRequest"/>
        </div>
      </div>
      <br />
      <div v-if="errorInfo" class="alert alert-danger">
        <h3 class="alert-heading">Error!</h3>
        <p class="text-white">{{errorInfo}}</p>
      </div>
    </div>
  </b-modal>
</template>

<script>
  import Vue from "vue";
  import axios from "axios";
  export default {
    data() {
      return {
        editPublisher: null,
        newPublisherName: null,
        newBudget: null,
        newFreeGamesDropped: null,
        newWillNotReleaseGamesDropped: null,
        newWillReleaseGamesDropped: null,
        errorInfo: null
      }
    },
    props: ['leagueYear'],
    computed: {
      publishers() {
        return this.leagueYear.publishers;
      }
    },
    methods: {
      selectPublisher(selectedPublisher) {
        this.newPublisherName = selectedPublisher.publisherName;
        this.newBudget = selectedPublisher.budget;
        this.newFreeGamesDropped = selectedPublisher.freeGamesDropped;
        this.newWillNotReleaseGamesDropped = selectedPublisher.willNotReleaseGamesDropped;
        this.newWillReleaseGamesDropped = selectedPublisher.willReleaseGamesDropped;
      },
      makeEditRequest() {
        var model = {
          publisherID: this.editPublisher.publisherID,
          leagueID: this.leagueYear.leagueID,
          publisherName: this.newPublisherName,
          budget: this.newBudget,
          freeGamesDropped: this.newFreeGamesDropped,
          willNotReleaseGamesDropped: this.newWillNotReleaseGamesDropped,
          willReleaseGamesDropped: this.newWillReleaseGamesDropped
        };
        axios
          .post('/api/leagueManager/EditPublisher', model)
          .then(response => {
            this.$refs.managerEditPublishersFormRef.hide();
            this.$emit('publishersEdited');
          })
          .catch(response => {
            this.errorInfo = response.response.data;
          });
      },
      clearData() {
        this.editPublisher = null;
      }
    }
  }
</script>
<style scoped>
  .submit-button {
    width: 100%;
  }
</style>
