<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <div>
        <h1>Edit Master Game</h1>
        <b-button variant="info" :to="{ name: 'activeMasterGameChangeRequests' }">View master change game requests</b-button>
        <b-button variant="info" :to="{ name: 'adminConsole' }">Admin Console</b-button>
      </div>
      <h2>{{masterGame.gameName}}</h2>
      <h3>{{masterGame.masterGameID}}</h3>
      <hr />
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
        masterGame: null
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
      }
    },
    mounted() {
      this.fetchMasterGame();
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
