<template>
    <div>
        <h2>Manage User Account</h2>
        <hr />
        <b-button variant="info" v-if="!accountConfirmed" v-on:click="sendConfirmationEmail">Resend Confirmation Email</b-button>
    </div>
</template>
<script>
    import axios from 'axios';

    export default {
        computed: {
            accountConfirmed() {
                return this.$store.getters.userInfo.emailConfirmed;
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
            }
        }
    }
</script>
