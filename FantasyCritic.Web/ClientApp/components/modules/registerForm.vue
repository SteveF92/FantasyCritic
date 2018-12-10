<template>
  <div>
    <form method="post" class="form-horizontal" role="form" v-on:submit.prevent="register">
      <div class="alert alert-danger" v-if="errorInfo">An error has occurred.</div>
      <div class="form-group">
        <label for="userName" class="control-label">Username</label>
        <input v-model="userName" v-validate="'required'" id="userName" name="userName" type="text" class="form-control input" />
        <span>{{ errors.first('userName') }}</span>
      </div>
      <div class="form-group">
        <label for="emailAddress" class="control-label">Email</label>
        <input v-model="emailAddress" v-validate="'required|email'" id="emailAddress" name="emailAddress" type="text" class="form-control input" />
        <span>{{ errors.first('emailAddress') }}</span>
      </div>
      <div class="form-group">
        <label for="password" class="control-label">Password</label>
        <input v-model="password" v-validate="'required|min:8'" name="password" type="password" class="form-control input" ref="password">
        <span>{{ errors.first('password') }}</span>
      </div>

      <div class="form-group">
        <label for="confirmPassword" class="control-label">Confirm Password</label>
        <input v-model="confirmPassword" v-validate="'required|confirmed:password'" name="confirmPassword" type="password" class="form-control input" data-vv-as="password">
        <span>{{ errors.first('confirmPassword') }}</span>
      </div>

      <div class="form-group">
        <div class="offset-md-10 col-md-2">
          <input type="submit" class="btn btn-primary" value="Register" :disabled="!formIsValid" />
        </div>
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
                userName: "",
                emailAddress: "",
                password: "",
                confirmPassword: "",
                errorInfo: ""
            }
        },
        computed: {
          formIsValid() {
            return !Object.keys(this.fields).some(key => this.fields[key].invalid);
          }
        },
        methods: {
            register() {
                var model = {
                    userName: this.userName,
                    emailAddress: this.emailAddress,
                    password: this.password,
                    confirmPassword: this.confirmPassword
                };
                this.$store.dispatch("registerAccount", model)
                  .then(() => {
                    this.$router.push({ name: "login", props: { accountCreated: true } });
                  })
                  .catch(returnedError => {
                    this.errorInfo = returnedError;
                  });
            }
        }
    }
</script>
