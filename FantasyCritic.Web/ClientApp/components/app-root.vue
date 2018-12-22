<template>
  <div id="vue-app">
    <div id="top-nav-bar">
      <top-navbar></top-navbar>
    </div>
    <div class="container-fluid">
      <div class="row">
        <main class="col-12">
          <router-view></router-view>
        </main>
      </div>
    </div>
    <footer class="footer">
      <div class="container">
        <span class="text-muted">Fantasy Critic - Alpha Version</span>
      </div>
    </footer>
  </div>
</template>

<script>
  import TopNavbar from "components/modules/topNavbar";

  export default {
    components: {
      TopNavbar
    },
    mounted() {
      if (this.$store.getters.tokenIsCurrent()) {
        this.$store.dispatch("getUserInfo");
      }
    }
  };
</script>

<style>
  [v-cloak] {
    display: none;
  }
</style>

<style lang="scss">
  /*@import '../scss/custom-bootstrap.scss';*/
  @import '../node_modules/bootstrap/scss/bootstrap.scss';
</style>

<style>
@import '../css/footer.css';
@import '../css/site.css';
</style>
