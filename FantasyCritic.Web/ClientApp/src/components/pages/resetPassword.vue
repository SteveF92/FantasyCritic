<template>
  <div>
    <div v-if="errorInfo" class="alert alert-danger" role="alert">
      {{errorInfo}}
    </div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <h1>Reset Password</h1>
      <hr />
      <ValidationObserver v-slot="{ handleSubmit, invalid }">
        <form @submit.prevent="handleSubmit(resetPassword)" class="form-horizontal col-lg-8 offset-lg-2 col-md-12 offset-md-0">
          <ValidationProvider rules="required" v-slot="{ errors }" name="Email Address">
            <div class="form-group">
              <label for="emailAddress" class="control-label">Email Address</label>
              <input v-model="emailAddress" id="emailAddress" name="emailAddress" type="text" class="form-control input" />
              <span class="text-danger">{{ errors[0] }}</span>
            </div>
          </ValidationProvider>
          <ValidationProvider rules="required|min:8||max:200|password:@Confirm" v-slot="{ errors }" name="New Password">
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

          <div class="right-button">
            <input type="submit" class="btn btn-primary" value="Reset Password" :disabled="invalid" />
          </div>
        </form>
      </ValidationObserver>
    </div>
  </div>
</template>

<script>
import Vue from 'vue';
import axios from 'axios';

export default {
  data() {
    return {
      emailAddress: '',
      newPassword: '',
      confirmNewPassword: '',
      errorInfo: ''
    };
  },
  methods: {
    resetPassword() {
      var model = {
        emailAddress: this.emailAddress,
        password: this.newPassword,
        confirmPassword: this.confirmNewPassword,
        code: this.$route.query.Code
      };
      axios
        .post('/api/account/ResetPassword', model)
        .then(response => {
          this.$router.push({ name: 'login' });
        })
        .catch(returnedError => {
          this.errorInfo = 'There was an error with your request.';
        });
    }
  }
};
</script>
