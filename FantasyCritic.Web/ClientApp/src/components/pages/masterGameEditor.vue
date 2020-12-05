<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <div>
        <h1>Edit Master Game</h1>
        <b-button variant="info" :to="{ name: 'activeMasterGameChangeRequests' }">View master change game requests</b-button>
        <b-button variant="info" :to="{ name: 'adminConsole' }">Admin Console</b-button>
      </div>
      <hr />
      <div v-if="masterGame">
        <h2>{{masterGame.gameName}}</h2>
        <div class="row" v-if="changeRequest">
          <div class="col-lg-10 col-md-12 offset-lg-1 text-well">
            <h2>Request Note</h2>
            <p>{{changeRequest.requestNote}}</p>
          </div>
          <hr />
        </div>
      </div>
    </div>
  </div>
</template>
<script>
  import axios from 'axios';
  import vueSlider from 'vue-slider-component';
  import Popper from 'vue-popperjs';
  import moment from 'moment';
  import 'vue-slider-component/theme/antd.css';
  import MasterGameTagSelector from '@/components/modules/masterGameTagSelector';

  export default {
    props: ['mastergameid'],
    data() {
      return {
        masterGame: null,
        changeRequest: null
      };
    },
    components: {
      vueSlider,
      'popper': Popper,
      MasterGameTagSelector
    },
    methods: {
      fetchMasterGame() {
        axios
          .get('/api/game/MasterGame/' + this.mastergameid)
          .then(response => {
            this.masterGame = response.data;
          })
          .catch(returnedError => (this.error = returnedError));
      },
      fetchChangeRequest() {
        let changeRequestID = this.$route.query.changeRequestID;
        if (!changeRequestID) {
          return;
        }
        axios
          .get('/api/admin/GetMasterGameChangeRequest?changeRequestID=' + changeRequestID)
          .then(response => {
            this.changeRequest = response.data;
          })
          .catch(returnedError => (this.error = returnedError));
      }
    },
    mounted() {
      this.fetchMasterGame();
      this.fetchChangeRequest();
    },
    watch: {
      '$route'(to, from) {
        this.fetchMasterGame();
      }
    }
  };
</script>
<style scoped>
  .select-cell {
    text-align: center;
  }

  .eligibility-explanation {
    margin-bottom: 50px;
    max-width: 1300px;
  }

  .eligibility-section {
    margin-bottom: 30px;
  }

  .eligibility-description {
    margin-top: 25px;
  }

  .checkbox-label {
    padding-left: 25px;
  }

  label {
    font-size: 18px;
  }
</style>
<style>
  .vue-slider-piecewise-label {
    color: white !important;
  }
</style>
