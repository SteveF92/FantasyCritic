<template>
  <b-modal id="deleteUserAccountForm" ref="deleteUserAccountFormRef" title="Delete Account" @hidden="clearData">
    <div class="alert alert-danger">
      <h2>Warning!</h2>
      <div>Deleting your account is not reverseable. If you click this button, the following will occur:</div>
      <ul>
        <li>All of your Royale Publishers will be deleted.</li>
        <li>Any publishers you have in a normal league will be marked as 'Deleted', but the games will remain, as to not affect the other players.</li>
        <li>All of your personal information, including display name, email address, and hashed password, will be deleted</li>
        <li>You will not be able to log in anymore.</li>
      </ul>
    </div>
    <p>If you are sure, type <strong>DELETE MY ACCOUNT</strong> into the box below and click the button.</p>

    <input v-model="confirmDelete" type="text" class="form-control input" />
    <div slot="modal-footer">
      <input type="submit" class="btn btn-danger" value="Delete Account" v-on:click="deleteAccount" :disabled="!deleteConfirmed" />
    </div>
  </b-modal>
</template>
<script>
import Vue from 'vue';
import axios from 'axios';

export default {
  data() {
    return {
      confirmDelete: ""
    };
  },
  computed: {
    deleteConfirmed() {
      let lowerCase = this.confirmDelete.toUpperCase();
      return lowerCase === 'DELETE MY ACCOUNT';
    }
  },
  methods: {
    deleteAccount() {
      axios
        .post('/api/account/DeleteUserAccount', model)
        .then(response => {
          this.$store.dispatch('logout')
            .then(() => {
              this.$router.push({ name: 'login' });
            });
        })
        .catch(response => {
        });
    },
    clearData() {
      this.confirmDelete = '';
    }
  }
};
</script>
