<template>
    <div>
        <h2>Change Email Address</h2>
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
                axios
                  .post('/api/account/ChangeEmail', model)
                  .then(response => {
                      this.$router.push({ name: "login" });
                  })
                  .catch(response => {

                  });
            }
        },
        mounted() {
          this.newEmailAddress = this.$route.query.NewEmailAddress;
          this.code = this.$route.query.Code;
        }
    }
</script>
