<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <h1>Reset Password</h1>
      <hr />
      <form method="post" class="form-horizontal col-lg-8 offset-lg-2 col-md-12 offset-md-0" role="form" v-on:submit.prevent="resetPassword">
        <div class="alert alert-danger" v-if="errorInfo">An error has occurred.</div>
        <div class="form-group">
          <label for="emailAddress" class="control-label">Email</label>
          <input v-model="emailAddress" id="emailAddress" name="emailAddress" type="text" class="form-control input" />
        </div>
        <div class="form-group">
          <label for="password" class="control-label">Password (Must be at least 12 characters)</label>
          <input v-model="password" id="password" name="password" type="password" class="form-control input" />
        </div>
        <div class="form-group">
          <label for="cPassword" class="control-label">Confirm Password</label>
          <input v-model="confirmPassword" id="cPassword" name="cPassword" type="password" class="form-control input" />
        </div>
        <div class="form-group">
          <div class="right-button">
            <input type="submit" class="btn btn-primary" value="Reset Password" />
          </div>
        </div>
      </form>
    </div>
  </div>
</template>

<script>
    import Vue from 'vue';
    import axios from 'axios';

    export default {
        data() {
            return {
                emailAddress: "",
                password: "",
                confirmPassword: "",
                errorInfo: ""
            }
        },
        methods: {
            resetPassword() {
                var model = {
                    emailAddress: this.emailAddress,
                    password: this.password,
                    confirmPassword: this.confirmPassword,
                    code: this.$route.query.Code
                };
                axios
                    .post('/api/account/ResetPassword', model)
                    .then(response => {
                        this.$router.push({ name: "login" });
                    })
                    .catch(response => {

                    });
            }
        }
    }
</script>
