<template>
  <div>
    <div v-if="errorInfo" class="alert alert-danger" role="alert">
      {{errorInfo}}
    </div>
    <ValidationObserver v-slot="{ invalid }">
      <form method="post" class="form-horizontal" role="form" v-on:submit.prevent="register">
        <div class="alert alert-danger" v-if="errorInfo">An error has occurred: {{errorInfo}}</div>
        <div class="form-group">
          <label for="emailAddress" class="control-label">Email Address</label>
          <ValidationProvider rules="required|email" v-slot="{ errors }" name="Email Address">
            <input v-model="emailAddress" id="emailAddress" name="emailAddress" type="text" class="form-control input" />
            <span class="text-danger">{{ errors[0] }}</span>
          </ValidationProvider>
        </div>

        <div class="form-group">
          <label for="displayName" class="control-label">Display Name</label>
          <ValidationProvider rules="required|max:30" v-slot="{ errors }" name="Display Name">
            <input v-model="displayName" id="displayName" name="displayName" type="text" class="form-control input" />
            <span class="text-danger">{{ errors[0] }}</span>
          </ValidationProvider>
        </div>

        <div class="form-group">
          <label for="password" class="control-label">Password</label>
          <ValidationProvider rules="required|min:8|max:80|password:@confirmPassword" v-slot="{ errors }">
            <input v-model="password" name="password" type="password" class="form-control input">
            <span class="text-danger">{{ errors[0] }}</span>
          </ValidationProvider>
        </div>

        <div class="form-group">
          <label for="confirmPassword" class="control-label">Confirm Password</label>
          <ValidationProvider name="confirmPassword" rules="required" v-slot="{ errors }">
            <input v-model="confirmPassword" name="confirmPassword" type="password" class="form-control input">
            <span class="text-danger">{{ errors[0] }}</span>
          </ValidationProvider>
        </div>

        <div class="alert alert-info" v-show="emailAddress">By using this site, you agree to our <a href="/about#privacy" target="_blank">Privacy Policy</a>.</div>

        <div class="register-button-area">
          <button type="submit" class="btn btn-primary register-button" :disabled="invalid">
            Sign Up
            <font-awesome-icon icon="user-plus" />
          </button>
        </div>
      </form>
    </ValidationObserver>
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
                    this.errorInfo = "There was an error with your request.";
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
