<template>
  <div>
    <form method="post" class="form-horizontal" role="form" v-on:submit.prevent="register">
      <div class="alert alert-danger" v-if="errorInfo">An error has occurred: {{errorInfo}}</div>
      <div class="form-group">
        <label for="emailAddress" class="control-label">Email</label>
        <input v-model="emailAddress" v-validate="'required|email'" id="emailAddress" name="emailAddress" type="text" class="form-control input" />
        <span class="text-danger">{{ errors.first('emailAddress') }}</span>
      </div>

      <div class="form-group">
        <label for="displayName" class="control-label">Display Name</label>
        <input v-model="displayName" v-validate="'required'" id="displayName" name="displayName" type="text" class="form-control input" />
        <span class="text-danger">{{ errors.first('displayName') }}</span>
      </div>

      <div class="form-group">
        <label for="password" class="control-label">Password</label>
        <input v-model="password" v-validate="'required|min:8'" name="password" type="password" class="form-control input" ref="password">
        <span class="text-danger">{{ errors.first('password') }}</span>
      </div>

      <div class="form-group">
        <label for="confirmPassword" class="control-label">Confirm Password</label>
        <input v-model="confirmPassword" v-validate="'required|confirmed:password'" name="confirmPassword" type="password" class="form-control input" data-vv-as="password">
        <span class="text-danger">{{ errors.first('confirmPassword') }}</span>
      </div>

      <div class="alert alert-info" v-show="emailAddress">By using this site, you agree to our <a href="/about#privacy" target="_blank">Privacy Policy</a>.</div>

      <div class="register-button-area">
        <button type="submit" class="btn btn-primary register-button" :disabled="!formIsValid">
          Sign Up
          <font-awesome-icon icon="user-plus" />
        </button>
      </div>
    </form>
  </div>
</template>

<script>
    import Vue from 'vue';
    import axios from 'axios';

    export default {
        data() {
            return {
              emailAddress: "",
              displayName: "",
              password: "",
              confirmPassword: "",
              errorInfo: ""
            }
        },
        computed: {
          formIsValid() {
            return !Object.keys(this.veeFields).some(key => this.veeFields[key].invalid);
          }
        },
        methods: {
            register() {
                var model = {
                  emailAddress: this.emailAddress,
                  displayName: this.displayName,
                  password: this.password,
                  confirmPassword: this.confirmPassword
                };
                this.$store.dispatch("registerAccount", model)
                  .then(() => {
                    let leagueid = this.$route.query.leagueid;
                    let inviteCode = this.$route.query.inviteCode;
                    let year = this.$route.query.year;
                    if (leagueid && inviteCode && year) {
                    let routeObject = { name: 'login', query: { leagueid: leagueid, year: year, inviteCode: inviteCode } };
                      this.$router.push(routeObject);
                    } else {
                      this.$router.push({ name: "login" });
                    }
                  })
                  .catch(returnedError => {
                    this.errorInfo = returnedError.response.data[0].description.replace("User name", "Email Address");
                  });
            }
        }
    }
</script>
<style scoped>
  .register-button-area {
    display: flex;
    margin-left: auto;
  }
  .register-button {
    margin-left: auto;
  }
</style>
