<template>
  <b-modal id="managerEditPublishersForm" ref="managerEditPublishersFormRef" size="lg" title="Edit Publishers" hide-footer @hidden="clearData">
    <div class="alert alert-warning">Warning! This feature is intended to fix mistakes and other exceptional circumstances. In general, managers should not be editing Publisher details.</div>
  </b-modal>
</template>

<script>
  import Vue from "vue";
  import axios from "axios";
  export default {
      data() {
          return {
          }
      },
      props: ['leagueYear'],
      methods: {
        clearData() {

        }
      }
  }
</script>
