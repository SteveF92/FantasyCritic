<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <h1>Register</h1>
      <hr />
      <div class="col-8 offset-2">
        <registerForm></registerForm>
      </div>
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
