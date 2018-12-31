<template>
    <div>
        <h1>Register</h1>
        <hr />
        <div class="col-md-6">
          <registerForm></registerForm>
        </div>
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
