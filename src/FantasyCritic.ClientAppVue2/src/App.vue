<template>
  <BApp :defaults="bootstrapDefaults">
    <div id="vue-app">
      <div id="top-nav-bar">
        <top-navbar></top-navbar>
      </div>
      <div>
        <div class="container-fluid">
          <div class="row">
            <main class="col-12 main-content">
              <router-view></router-view>
            </main>
          </div>
        </div>
        <footerModule></footerModule>
      </div>
      <toastHost></toastHost>
    </div>
  </BApp>
</template>

<script>
import TopNavbar from '@/components/topNavBar.vue';
import FooterModule from '@/components/footerModule.vue';
import ToastHost from '@/components/toastHost.vue';
import { defaultTableComparer } from '@/globalFunctions';

export default {
  components: {
    TopNavbar,
    FooterModule,
    ToastHost
  },
  data() {
    return {
      // Bootstrap-Vue 2 tables toggled between ascending and descending. bootstrap-vue-next clears the sort on a third click unless mustSort is set.
      // Bootstrap 4 dropped dropdown menus 0.125rem below their toggle; bootstrap-vue-next positions them with no gap.
      bootstrapDefaults: {
        BTable: { mustSort: true, sortCompare: defaultTableComparer },
        BDropdown: { offset: 2 }
      }
    };
  }
};
</script>

<style lang="scss">
@import './scss/custom-bootstrap.scss';
@import '../node_modules/bootstrap/scss/bootstrap.scss';
@import './scss/bootstrap-overrides.scss';
</style>

<style>
@import './css/site.css';
@import './css/footer.css';
@import './css/modal.css';
</style>
