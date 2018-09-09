<template>
  <div>
    <h2>Forgot Password</h2>
    <div v-if="showSent">Forgot password email has been sent. Please check your email.</div>
    <form v-on:submit.prevent="sendForgotPasswordRequest">
      <div class="form-group col-md-4">
        <label for="emailAddress" class="control-label">Email Address</label>
        <input v-model="emailAddress" type="text" class="form-control input" />
      </div>
      <div class="form-group">
        <div class="col-md-offset-2 col-md-4">
          <input type="submit" class="btn btn-default" value="Submit" />
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
