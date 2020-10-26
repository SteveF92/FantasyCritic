<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <h1>Forgot Password</h1>
      <hr />

      <div v-if="showSent" class="alert alert-success">Forgot password email has been sent. Please check your email.</div>
      <ValidationObserver v-slot="{ invalid }">
        <form v-on:submit.prevent="sendForgotPasswordRequest" class="col-lg-8 offset-lg-2 col-md-12 offset-md-0">
          <div class="form-group">
            <label for="emailAddress" class="control-label">Email Address</label>
            <ValidationProvider rules="required|email" v-slot="{ errors }">
              <input v-model="emailAddress" id="emailAddress" name="Email Address" type="text" class="form-control input" />
              <span class="text-danger">{{ errors[0] }}</span>
            </ValidationProvider>
          </div>
          <div class="form-group">
            <div class="right-button">
              <input type="submit" class="btn btn-primary" value="Submit" :disabled="invalid" />
            </div>
          </div>
        </form>
      </ValidationObserver>
    </div>
  </div>
</template>
<script>
import axios from 'axios';

export default {
    data() {
        return {
            emailAddress: '',
            showSent: false
        };
    },
    methods: {
        sendForgotPasswordRequest() {
            let request = {
                emailAddress: this.emailAddress
            };
            axios
                .post('/api/account/ForgotPassword', request)
                .then(response => {
                    this.showSent = true;
                })
                .catch(response => {

                });
        }
    }
};
</script>
