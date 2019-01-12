<template>
  <div>
    <form method="post" class="form-horizontal" role="form" v-on:submit.prevent="register">
      <div class="alert alert-danger" v-if="errorInfo">An error has occurred.</div>
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
                    this.$router.push({ name: "login" });
                  })
                  .catch(returnedError => {
                    this.errorInfo = returnedError;
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
