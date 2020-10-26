<template>
  <b-modal id="changeEmailForm" ref="changeEmailRef" title="Change Email Address" @hidden="clearData">
    <div class="form-horizontal">
      <div class="form-group">
        <label for="newEmail" class="control-label">New Email Address</label>
        <input v-model="newEmail" id="newEmail" name="newEmail"  class="form-control input" />
      </div>
    </div>
    <div slot="modal-footer">
      <input type="submit" class="btn btn-primary" value="Change Email Address" v-on:click="sendChangeEmail" :disabled="!formValid" />
    </div>
  </b-modal>
</template>
<script>
import Vue from 'vue';
import axios from 'axios';

export default {
  data() {
    return {
      newEmail: '',
      errorInfo: ''
    };
  },
  computed: {
    formValid() {
      return this.newEmail;
    }
  },
  methods: {
    sendChangeEmail() {
      var model = {
        newEmailAddress: this.newEmail
      };
      axios
        .post('/api/account/SendChangeEmail', model)
        .then(response => {
          this.$refs.changeEmailRef.hide();
          this.$emit('sentEmailChanged');
          this.clearData();
        })
        .catch(response => {
        });
    },
    clearData() {
      this.newEmail = '';
    }
  }
};
</script>
