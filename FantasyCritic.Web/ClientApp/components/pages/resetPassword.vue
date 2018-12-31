<template>
    <div>
        <h1>Reset Password</h1>
        <hr />
        <form method="post" class="form-horizontal" role="form" v-on:submit.prevent="resetPassword">
            <div class="alert alert-danger" v-if="errorInfo">An error has occurred.</div>
            <div class="form-group col-md-10">
                <label for="emailAddress" class="control-label">Email</label>
                <input v-model="emailAddress" id="emailAddress" name="emailAddress" type="text" class="form-control input" />
            </div>
            <div class="form-group col-md-10">
                <label for="password" class="control-label">Password (Must be at least 12 characters)</label>
                <input v-model="password" id="password" name="password" type="password" class="form-control input" />
            </div>
            <div class="form-group col-md-10">
                <label for="cPassword" class="control-label">Confirm Password</label>
                <input v-model="confirmPassword" id="cPassword" name="cPassword" type="password" class="form-control input" />
            </div>
            <div class="form-group">
                <div class="col-md-offset-2 col-md-10">
                    <input type="submit" class="btn btn-primary" value="Reset Password" />
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
                emailAddress: "",
                password: "",
                confirmPassword: "",
                errorInfo: ""
            }
        },
        methods: {
            resetPassword() {
                var model = {
                    emailAddress: this.emailAddress,
                    password: this.password,
                    confirmPassword: this.confirmPassword,
                    code: this.$route.query.Code
                };
                axios
                    .post('/api/account/ResetPassword', model)
                    .then(response => {
                        this.$router.push({ name: "login" });
                    })
                    .catch(response => {

                    });
            }
        }
    }
</script>
