<template>
    <div>
        <h2>Register</h2>
        <hr />
        <registerForm></registerForm>
    </div>
</template>

<script>
    import Vue from 'vue';
    import axios from 'axios';
    import RegisterForm from "components/modules/registerForm";

    export default {
      components: {
        RegisterForm
      }
    }
</script>
