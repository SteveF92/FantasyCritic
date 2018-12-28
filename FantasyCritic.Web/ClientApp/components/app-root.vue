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
    <footerModule>
    </footerModule>
  </div>
</template>

<script>
  import TopNavbar from "components/modules/topNavbar";
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
  @import '../scss/custom-bootstrap.scss';
  @import '../../node_modules/bootstrap/scss/bootstrap.scss';
</style>

<style>
  @import '../css/site.css';
  @import '../css/modal.css';
</style>


<style>
    @media only screen and (max-width: 343px) {
      .full-contact {
        display: none;
      }
    }

    @media only screen and (max-width: 679px) {
      .full-logo {
        display: none;
      }

      #vue-app {
        margin-bottom: 75px; /* Margin bottom by footer height */
      }

      .footer {
        height: 75px; /* Set the fixed height of the footer here */
      }

      .container {
        padding-bottom: 25px;
      }
    }

    @media only screen and (min-width: 680px) {
      .small-logo {
        display: none;
      }

      #vue-app {
        margin-bottom: 175px; /* Margin bottom by footer height */
      }

      .footer {
        height: 175px; /* Set the fixed height of the footer here */
      }
    }

    .footer {
      position: absolute;
      background-color: black;
      bottom: 0;
      width: 100%;
      line-height: 30px; /* Vertically center the text there */
    }

    /* Custom page CSS
  -------------------------------------------------- */
    /* Not required for template or sticky footer method. */

    .container {
      width: auto;
      max-width: 1200px;
      padding-left: 45px;
      padding-right: 45px;
      background-color: black;
    }
</style>
