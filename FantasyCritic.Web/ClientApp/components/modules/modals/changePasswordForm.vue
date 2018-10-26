<template>
  <b-modal id="changePasswordForm" ref="changePasswordRef" title="Change Password" @hidden="clearData">
    <div class="form-horizontal">
      <div class="form-group">
        <label for="currentPassword" class="control-label">Current Password</label>
        <input v-model="currentPassword" id="currentPassword" name="currentPassword" type="password" class="form-control input" />
      </div>
      <div class="form-group">
        <label for="newPassword" class="control-label">New Password</label>
        <input v-model="newPassword" id="newPassword" name="newPassword" type="password" class="form-control input" />
      </div>
      <div class="form-group">
        <label for="confirmNewPassword" class="control-label">Confirm New Password</label>
        <input v-model="confirmNewPassword" id="confirmNewPassword" name="confirmNewPassword" type="password" class="form-control input" />
      </div>
    </div>
    <div slot="modal-footer">
      <input type="submit" class="btn btn-primary" value="Change Password" v-on:click="changePassword" :disabled="!formValid" />
    </div>
  </b-modal>
</template>
<script>
  import Vue from "vue";
  import axios from "axios";

  export default {
    data() {
      return {
        currentPassword: "",
        newPassword: "",
        confirmNewPassword: "",
        errorInfo: ""
      }
    },
    computed: {
      formValid() {
        if (this.newPassword !== this.confirmNewPassword) {
          return false;
        }

        return this.currentPassword && this.newPassword && this.confirmNewPassword;
      }
    },
    methods: {
      changePassword() {
        var model = {
          currentPassword: this.currentPassword,
          newPassword: this.newPassword,
          confirmNewPassword: this.confirmNewPassword
        };
        axios
          .post('/api/account/changePassword', model)
          .then(response => {
            this.$refs.changePasswordRef.hide();
            this.$emit('passwordChanged');
            this.clearData();
          })
          .catch(response => {
          });
      },
      clearData() {
        this.currentPassword = "";
        this.newPassword = "";
        this.confirmNewPassword = "";
      }
    }
  }
</script>
