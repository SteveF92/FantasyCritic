<template>
  <b-modal id="transferManagerForm" ref="transferManagerFormRef" size="lg" title="Transfer Manager" hide-footer @hidden="clearData">
    Promoting a new Manager?
  </b-modal>
</template>

<script>
  import axios from 'axios';
  export default {
    data() {
      return {

      };
    },
    props: ['league','leagueYear'],
    computed: {
      publishers() {
        return this.leagueYear.publishers;
      }
    },
    methods: {
      clearData() {

      }
    }
  };
</script>
