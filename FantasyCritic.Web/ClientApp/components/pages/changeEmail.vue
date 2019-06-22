<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <h1>Change Email Address</h1>
      <hr />
      <form method="post" class="form-horizontal" role="form" v-on:submit.prevent="changeEmail">
        <div class="alert alert-warning">Click the button below to change your email address to {{newEmailAddress}}</div>
        <div class="form-group">
          <div class="col-md-offset-2 col-md-10">
            <input type="submit" class="btn btn-primary" value="Change Email Address" />
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
              newEmailAddress: "",
              code: ""
            }
        },
        methods: {
            changeEmail() {
              var model = {
                newEmailAddress: this.newEmailAddress,
                code: this.code
              };
              this.$store.dispatch("changeEmailAddress", model)
                .then(() => {
                  this.$router.push({ name: "login" });
                })
                .catch(returnedError => {
                });
            }
        },
        mounted() {
          this.newEmailAddress = this.$route.query.NewEmailAddress;
          this.code = this.$route.query.Code;
        }
    }
</script>
