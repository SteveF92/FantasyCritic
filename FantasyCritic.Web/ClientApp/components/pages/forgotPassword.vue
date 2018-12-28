<template>
    <div>
      <h2>Forgot Password</h2>
      <div v-if="showSent" class="alert alert-success">Forgot password email has been sent. Please check your email.</div>
      <form v-on:submit.prevent="sendForgotPasswordRequest">
        <div class="form-group col-md-4">
          <label for="emailAddress" class="control-label">Email Address</label>
          <input v-model="emailAddress" v-validate="'required|email'" type="text" class="form-control input" />
          <span class="text-danger">{{ errors.first('emailAddress') }}</span>
        </div>
        <div class="form-group">
          <div class="col-md-offset-2 col-md-4">
            <input type="submit" class="btn btn-primary" value="Submit" :disabled="!formIsValid" />
          </div>
        </div>
      </form>
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
      return !Object.keys(this.fields).some(key => this.fields[key].invalid);
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
