<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <h1>Login</h1>
      <hr />
      <div class="alert alert-success" v-show="newAccountCreated">
        <div>Your account was sucessfully created. Check your account for an email from us to confirm your email address.</div>
      </div>
      <div class="row">
        <div class="col-8 offset-2">
          <div v-if="isBusy">
            <i class="fa fa-circle-o-notch fa-spin"></i>
          </div>
          <form v-on:submit.prevent="login">
            <div class="form-group col-md-10">
              <label for="emailAddress" class="control-label">Email Address</label>
              <input v-model="emailAddress" type="text" class="form-control input" />
            </div>
            <div class="form-group col-md-10">
              <label for="password" class="control-label">Password</label>
              <input v-model="password" type="password" class="form-control input" />
            </div>
            <div class="alert alert-danger" v-if="error">Login failed</div>
            <div class="form-group">
              <div class="col-3 offset-8">
                <div>
                  <button type="submit" class="btn btn-primary">
                    Log in
                    <font-awesome-icon icon="sign-in-alt" />
                  </button>
                </div>
                <router-link :to="{ name: 'forgotPassword' }">Forgot your password?</router-link>
              </div>
            </div>

          </form>
        </div>
      </div>
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
                isBusy: false,
                error: ""
            }
        },
        computed: {
          newAccountCreated() {
            return this.$store.getters.newAccountCreated;
          }
        },
        methods: {
            login() {
                var credentials = {
                    emailAddress: this.emailAddress,
                    password: this.password
                };
                this.$store.dispatch("doAuthentication", credentials)
                    .then(() => {
                        this.isBusy = false;
                        let redirect = this.$store.getters.redirect;
                        if (!redirect) {
                            redirect = "/";
                        }
                        this.$store.commit("clearRedirect");
                        this.$router.push(redirect);
                    })
                    .catch(returnedError => {
                        this.isBusy = false;
                        this.error = "Failed Login";
                    });
            }
        }
    }
</script>
