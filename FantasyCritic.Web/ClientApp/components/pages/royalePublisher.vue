<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <div v-if="publisher">
      <div class="row">
        <div class="col-md-12 col-lg-6">
          <div class="publisher-name">
            <h1>{{publisher.publisherName}}</h1>
          </div>
          <h4>Player Name: {{publisher.playerName}}</h4>
          <h4>
            Year/Quarter: {{publisher.yearQuarter.year}}-Q{{publisher.yearQuarter.quarter}}
          </h4>
        </div>

        <div class="col-md-12 col-lg-6 text-well top-area">
          <div class="row main-buttons">
            <b-button variant="primary" class="main-button">Add a Game</b-button>
            <b-button variant="warning" class="main-button">Sell a Game</b-button>
          </div>
        </div>
      </div>
      
      <h1>Stuff!</h1>
      
    </div>
  </div>
</template>

<script>
  import Vue from "vue";
  import axios from "axios";

  export default {
    props: ['publisherID'],
    data() {
      return {
          errorInfo: "",
          publisher: null
      }
    },
    components: {
    },
    props: ['publisherid'],
    methods: {
        fetchPublisher() {
            axios
                .get('/api/Royale/GetRoyalePublisher/' + this.publisherid)
                .then(response => {
                  this.publisher = response.data;
                })
                .catch(returnedError => (this.error = returnedError));
        }
    },
    mounted() {
        this.fetchPublisher();
    },
    watch: {
      '$route'(to, from) {
          this.fetchPublisher();
      }
    }
  }
</script>
<style scoped>
  .publisher-name {
    display: block;
    max-width: 100%;
    word-wrap: break-word;
  }
  .top-area{
    margin-top: 10px;
    margin-bottom: 20px;
  }

  .main-buttons {
    display: flex;
    flex-direction: row;
    justify-content: space-around;
  }

  .main-button{
    margin-top: 5px;
    min-width: 200px;
  }
</style>
