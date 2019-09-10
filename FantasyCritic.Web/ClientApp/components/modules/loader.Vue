<template>
 <div id="app-loader" v-if="loading">
    <div class="loader"></div>
 </div>
</template>
<script>
export default {
  name: 'app-loader',
  props: { loading: Boolean }
}
</script>
<style lang="scss" scoped>
  /* Adding variables to the scss loader would be perferable */
  @import '../../scss/custom-bootstrap.scss';

  #app-loader {
    height: 100%;
    display: flex;
    align-items: center;
    justify-content: center;

    .loader {
      border: 16px solid $secondary;
      border-radius: 50%;
      border-top: 16px solid $primary;
      width: 140px;
      height: 140px;
      -webkit-animation: spin 1s linear infinite; /* Safari */
      animation: spin 1s linear infinite;
    }

    /* Safari */
    @-webkit-keyframes spin {
      0% { -webkit-transform: rotate(0deg); }
      100% { -webkit-transform: rotate(360deg); }
    }

    @keyframes spin {
      0% { transform: rotate(0deg); }
      100% { transform: rotate(360deg); }
    }

  }

</style>
