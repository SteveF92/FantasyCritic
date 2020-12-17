<template>
  <b-modal id="managerMessageForm" ref="managerMessageFormRef" size="lg" title="Post New Message to League" hide-footer @hidden="clearData">
    Post a message!
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
