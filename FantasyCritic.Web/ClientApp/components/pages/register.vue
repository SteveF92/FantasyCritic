<template>
    <div>
        <h2>Register</h2>
        <hr />
        <form method="post" class="form-horizontal" role="form" v-on:submit.prevent="register">
            <div class="alert alert-danger" v-if="errorInfo">An error has occurred.</div>
            <div class="form-group col-md-10">
                <label for="userName" class="control-label">User name</label>
                <input v-model="userName" id="userName" name="userName" type="text" class="form-control input" />
            </div>
            <div class="form-group col-md-10">
                <label for="realName" class="control-label">Real name (optional)</label>
                <input v-model="realName" id="realName" name="realName" type="text" class="form-control input" />
            </div>
            <div class="form-group col-md-10">
                <label for="emailAddress" class="control-label">Email</label>
                <input v-model="emailAddress" id="emailAddress" name="emailAddress" type="text" class="form-control input" />
            </div>
            <div class="form-group col-md-10">
                <label for="password" class="control-label">Password</label>
                <input v-model="password" id="password" name="password" type="password" class="form-control input" />
            </div>
            <div class="form-group col-md-10">
                <label for="cPassword" class="control-label">Confirm Password</label>
                <input v-model="confirmPassword" id="cPassword" name="cPassword" type="password" class="form-control input" />
            </div>
            <div class="form-group">
                <div class="col-md-offset-2 col-md-10">
                    <input type="submit" class="btn btn-primary" value="Register" />
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
                realName: "",
                emailAddress: "",
                password: "",
                confirmPassword: "",
                errorInfo: ""
            }

        },
        methods: {
            register() {
                var model = {
                    userName: this.userName,
                    realName: this.realName,
                    emailAddress: this.emailAddress,
                    password: this.password,
                    confirmPassword: this.confirmPassword
                };
                axios
                    .post('/api/account/register', model)
                    .then(this.responseHandler)
                    .catch(this.catchHandler);
            },
            responseHandler(response) {

            },
            catchHandler(returnedError) {

            }
        }
    }
</script>
