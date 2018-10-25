<template>
  <b-modal id="createPublisher" ref="createPublisherRef" title="Create Publisher" @hidden="clearData">
    <form class="form-horizontal">
      <div class="form-group">
        <label for="publisherName" class="control-label">Publisher Name</label>
        <input v-model="publisherName" id="publisherName" name="publisherName" type="text" class="form-control input" />
      </div>
    </form>
    <div slot="modal-footer">
      <input type="submit" class="btn btn-primary" value="Create Publisher" v-on:click="createPublisher" :disabled="!publisherName"/>
    </div>
  </b-modal>
</template>
<script>
  import Vue from "vue";
  import axios from "axios";

  export default {
    data() {
      return {
        publisherName: "",
        errorInfo: ""
      }
    },
    props: ['leagueYear'],
    methods: {
      createPublisher() {
        var model = {
          leagueID: this.leagueYear.leagueID,
          year: this.leagueYear.year,
          publisherName: this.publisherName
        };
        axios
          .post('/api/league/createPublisher', model)
          .then(response => {
            this.$refs.createPublisherRef.hide();
            let actionInfo = {
              message: this.publisherName + ' created.',
              fetchLeagueYear: true
            };
            this.$emit('actionTaken', actionInfo);
            this.publisherName = "";
          })
          .catch(response => {
            
          });
      },
      clearData() {
        this.publisherName = "";
      }
    }
  }
</script>
