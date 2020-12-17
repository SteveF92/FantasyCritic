<template>
  <b-modal id="removePlayerForm" ref="removePlayerFormRef" size="lg" title="RemovePlayers" hide-footer @hidden="clearData">
    Removing players?
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
