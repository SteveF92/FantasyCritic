<template>
  <div>
  </div>
</template>
<script>
import Vue from "vue";
import axios from "axios";

export default {
    data() {
      return {
        errorInfo: "",
        leagueActions: [],
        piecewiseStyle: {
          "backgroundColor": "#ccc",
          "visibility": "visible",
          "width": "12px",
          "height": "20px"
        }
      }
    },
    props: ['leagueid', 'year'],
    components: {
      vueSlider
    },
    methods: {
      fetchLeagueActions() {
        axios
          .get('/api/League/GetLeagueActions?leagueID=' + this.leagueid + '&year=' + this.year)
          .then(response => {
            this.leagueActions = response.data;
          })
          .catch(returnedError => (this.error = returnedError));
      }
    },
    mounted() {
      this.fetchLeagueActions();
    }
}
</script>
