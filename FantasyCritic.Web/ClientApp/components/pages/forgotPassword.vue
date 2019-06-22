<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <h1>Forgot Password</h1>
      <hr />

      <div v-if="showSent" class="alert alert-success">Forgot password email has been sent. Please check your email.</div>
      <form v-on:submit.prevent="sendForgotPasswordRequest" class="col-lg-8 offset-lg-2 col-md-12 offset-md-0">
        <div class="form-group">
          <label for="emailAddress" class="control-label">Email Address</label>
          <input v-model="emailAddress" v-validate="'required|email'" type="text" class="form-control input" />
          <span class="text-danger">{{ errors.first('emailAddress') }}</span>
        </div>
        <div class="form-group">
          <div class="right-button">
            <input type="submit" class="btn btn-primary" value="Submit" :disabled="!formIsValid" />
          </div>
        </div>
      </form>
    </div>
  </div>
</template>
<script>
  import axios from 'axios';

  export default {
    data() {
      return {
        emailAddress: "",
        showSent: false
      }
  },
  computed: {
    formIsValid() {
      return !Object.keys(this.veeFields).some(key => this.veeFields[key].invalid);
    }
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
}
</script>
