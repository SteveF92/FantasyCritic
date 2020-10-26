<template>
  <b-modal id="changePasswordForm" ref="changePasswordRef" title="Change Password" @hidden="clearData" hide-footer>
    <div v-if="errorInfo" class="alert alert-danger" role="alert">
      {{errorInfo}}
    </div>
    <div class="form-horizontal">
      <ValidationObserver v-slot="{ handleSubmit, invalid }">
        <form @submit.prevent="handleSubmit(changePassword)">
          <ValidationProvider rules="required" v-slot="{ errors }" name="Current Password">
            <div class="form-group">
              <label for="currentPassword" class="control-label">Current Password</label>
              <input v-model="currentPassword" id="currentPassword" name="currentPassword" type="password" class="form-control input" />
              <span class="text-danger">{{ errors[0] }}</span>
            </div>
          </ValidationProvider>
          <ValidationProvider rules="required|min:8||max:80|password:@Confirm" v-slot="{ errors }" name="New Password">
            <div class="form-group">
              <label for="newPassword" class="control-label">New Password</label>
              <input v-model="newPassword" id="newPassword" name="newPassword" type="password" class="form-control input" />
              <span class="text-danger">{{ errors[0] }}</span>
            </div>
          </ValidationProvider>
          <ValidationProvider name="Confirm" rules="required" v-slot="{ errors }">
            <div class="form-group">
              <label for="confirmNewPassword" class="control-label">Confirm New Password</label>
              <input v-model="confirmNewPassword" id="confirmNewPassword" name="confirmNewPassword" type="password" class="form-control input" />
              <span class="text-danger">{{ errors[0] }}</span>
            </div>
          </ValidationProvider>

          <input type="submit" class="btn btn-primary modal-submit-button" value="Change Password" :disabled="invalid" />
        </form>
      </ValidationObserver>
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
          .catch(returnedError => {
            this.errorInfo = "There was an error with your request.";
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
