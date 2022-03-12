<template>
  <b-modal id="managerMessageForm" ref="managerMessageFormRef" size="lg" title="Post New Message to League" @hidden="clearData">
    <div class="form-horizontal">
      <div class="form-group">
        <label for="messageText" class="control-label">Message</label>
        <textarea class="form-control" v-model="messageText" rows="3"></textarea>
        <div class="form-check">
          <input type="checkbox" class="form-check-input" v-model="isPublic">
          <label class="form-check-label" for="isPublic">Show message to users not in league? (only applies in public leagues)</label>
        </div>
      </div>
    </div>
    <div slot="modal-footer">
      <input type="submit" class="btn btn-primary" value="Post Message" v-on:click="postNewMessage" :disabled="!messageText" />
    </div>
  </b-modal>
</template>

<script>
  import axios from 'axios';
  export default {
    data() {
      return {
        messageText: null,
        isPublic: false
      };
    },
    props: ['league','leagueYear'],
    computed: {

    },
    methods: {
      postNewMessage() {
        var model = {
          leagueID: this.league.leagueID,
          year: this.leagueYear.year,
          message: this.messageText,
          isPublic: this.isPublic
        };
        axios
          .post('/api/leagueManager/PostNewManagerMessage', model)
          .then(response => {
            this.$refs.managerMessageFormRef.hide();
            this.$emit('managerMessagePosted');
            this.clearData();
          })
          .catch(response => {
          });
      },
      clearData() {
        this.messageText = null;
      }
    }
  };
</script>
