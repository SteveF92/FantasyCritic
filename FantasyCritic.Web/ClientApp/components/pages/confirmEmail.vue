<template>
    <div>
        <h2 v-if="!accountConfirmed">Confirm Email</h2>
        <h2 v-if="accountConfirmed">Email Confirmation Successful</h2>
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
        mounted() {
            let request = {
                userID: this.$route.query.UserID,
                code: this.$route.query.Code
            };
            axios
                .post('/api/account/ConfirmEmail', request)
                .then(response => {
                    this.$store.dispatch("getUserInfo")
                        .then(() => {
                        })
                        .catch(returnedError => {
                        });
                })
                .catch(response => {

                });
        }
    }
</script>
