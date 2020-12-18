<template>
  <b-modal id="removePlayerForm" ref="removePlayerFormRef" size="lg" title="Remove Player" hide-footer @hidden="clearData">
    <div v-if="!leagueYear.playStatus.playStarted">
      <label>Use this option to remove a Publisher (does not remove the player).</label>
      <div class="form-group">
        <label for="publisherToRemove" class="control-label">Publisher to Remove</label>
        <b-form-select v-model="publisherToRemove">
          <option v-for="publisher in publishers" v-bind:value="publisher">
            {{ publisher.publisherName }}
          </option>
        </b-form-select>
      </div>
      <b-button variant="danger" v-show="publisherToRemove" v-on:click="removePublisher">Remove Publisher</b-button>
    </div>
    <hr/>
    <div>
      <label>Remove Player</label>
    </div>
  </b-modal>
</template>

<script>
  import axios from 'axios';
  export default {
    data() {
      return {
        publisherToRemove: null
      };
    },
    props: ['league','leagueYear'],
    computed: {
      publishers() {
        return this.leagueYear.publishers;
      }
    },
    methods: {
      removeUser(user) {
        var model = {
          leagueID: this.leagueYear.leagueID,
          userID: user.userID
        };
        axios
          .post('/api/leagueManager/RemovePlayer', model)
          .then(response => {
            let actionInfo = {
              message: 'User ' + user.displayName + ' has been removed from the league.',
              fetchLeague: true,
              fetchLeagueYear: true
            };
            this.$emit('actionTaken', actionInfo);
          })
          .catch(response => {

          });
      },
      removePublisher() {
        var model = {
          leagueID: this.leagueYear.leagueID,
          publisherID: this.publisherToRemove.publisherID
        };
        axios
          .post('/api/leagueManager/RemovePublisher', model)
          .then(response => {
            let actionInfo = {
              publisherName: this.publisherToRemove.publisherName
            };
            this.$emit('publisherRemoved', actionInfo);
          })
          .catch(response => {

          });
      },
      clearData() {

      }
    }
  };
</script>
