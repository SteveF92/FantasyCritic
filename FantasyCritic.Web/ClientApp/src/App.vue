<template>
  <div id="vue-app">
    <div id="top-nav-bar">
      <top-navbar></top-navbar>
    </div>
    <div>
      <div class="container-fluid">
        <div class="row">
          <main class="col-12">
            <router-view></router-view>
          </main>
        </div>
      </div>
      <footerModule>
      </footerModule>
    </div>
  </div>
</template>

<script>
  import TopNavbar from "components/modules/topNavBar";
  import FooterModule from "components/modules/footerModule";

  export default {
    components: {
      TopNavbar,
      FooterModule
    },
    mounted() {
      if (this.$store.getters.tokenIsCurrent()) {
        this.$store.dispatch("getUserInfo");
      }
    }
  };
</script>

<style lang="scss">
  @import './scss/custom-bootstrap.scss';
  @import '../node_modules/bootstrap/scss/bootstrap.scss';
</style>

<style>
  @import './css/site.css';
  @import './css/footer.css';
  @import './css/modal.css';
</style>
