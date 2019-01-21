<template>
    <div>
        <div v-if="!accountConfirmed" class="alert alert-info">
          <div>Confirming Email...</div>
          <div>If you are having issues, check out our <a href="/faq#bidding-system" target="_blank">FAQ</a> page.</div>
        </div>
        <div v-if="accountConfirmed" class="alert alert-success">
          <span>Email Confirmation Successful!</span>
          <span>You will be redirected in a few seconds...</span>
        </div>
    </div>
</template>
<script>
  import axios from 'axios';

  export default {
    computed: {
      accountConfirmed() {
        if (this.$store.getters.userInfo) {
          return this.$store.getters.userInfo.emailConfirmed;
        }
        return false;
      },
      isAuth() {
        return this.$store.getters.tokenIsCurrent();
      }
    },
    mounted() {
      let request = {
          userID: this.$route.query.UserID,
          code: this.$route.query.Code
      };
      axios
        .post('/api/account/ConfirmEmail', request)
        .then(response => {
          if (this.isAuth) {
            this.$store.dispatch("getUserInfo")
              .then(() => {
                setTimeout(() => this.$router.push({ name: "home" }), 3000);
              })
              .catch(returnedError => {
              });
          } else {
            setTimeout(() => this.$router.push({ name: "login" }), 3000); 
          }    
        })
        .catch(response => {

        });
    }
  }
</script>
