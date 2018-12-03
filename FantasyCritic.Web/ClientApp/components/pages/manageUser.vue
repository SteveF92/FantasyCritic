<template>
  <div>
    <h2>Manage User Account</h2>
    <hr />

    <dl class="row">
      <dt class="col-sm-3">Username</dt>
      <dd class="col-sm-9">
        {{userInfo.userName}}
        <b-button variant="info" v-b-modal="'changeUserNameForm'">Change Username</b-button>
      </dd>

      <dt class="col-sm-3">Email Address</dt>
      <dd class="col-sm-9">
        {{userInfo.emailAddress}}
        <b-button variant="info" v-if="!userInfo.emailConfirmed" v-on:click="sendConfirmationEmail">Resend Confirmation Email</b-button>
      </dd>

      <dt class="col-sm-3">Password</dt>
      <dd class="col-sm-9">
        <b-button variant="info" v-b-modal="'changePasswordForm'">Change Password</b-button>
      </dd>
    </dl>
    <changePasswordForm v-on:passwordChanged="passwordChanged"></changePasswordForm>
    <changeUserNameForm v-on:userNameChanged="userNameChanged"></changeUserNameForm>

  </div>
</template>
<script>
  import axios from 'axios';
  import ChangePasswordForm from "components/modules/modals/changePasswordForm";
  import ChangeUserNameForm from "components/modules/modals/changeUserNameForm";

  export default {
    components: {
      ChangePasswordForm,
      ChangeUserNameForm
    },
    computed: {
        userInfo() {
            return this.$store.getters.userInfo;
        }
    },
    methods: {
      sendConfirmationEmail() {
          axios
              .post('/api/account/ResendConfirmationEmail')
              .then(response => {
                  let toast = this.$toasted.show('Confirmation Email Sent!', {
                      theme: "primary",
                      position: "top-right",
                      duration: 5000
                  });
              })
              .catch(response => {

              });
      },
      passwordChanged() {
        let toast = this.$toasted.show("Your password has been changed.", {
          theme: "primary",
          position: "top-right",
          duration: 5000
        });
      },
      userNameChanged(newUserNameInfo) {
        let toast = this.$toasted.show("Your Username has been changed to " + newUserNameInfo.newUserName, {
          theme: "primary",
          position: "top-right",
          duration: 5000
        });
      }
    }
  }
</script>
