<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <div v-if="publisher">
      <div class="publisher-name">
        <h1>{{publisher.publisherName}}</h1>
      </div>
      <div class="row">
        <div class="col-md-12 col-lg-6">
          <h4>Player Name: {{publisher.playerName}}</h4>
          <h4>
            Year/Quarter: {{publisher.yearQuarter.year}}-Q{{publisher.yearQuarter.quarter}}
          </h4>
        </div>

        <div class="col-md-12 col-lg-6">
          <h4>Remaining Budget: {{publisher.budget | money}}</h4>
          <b-button variant="primary" v-b-modal="'royalePurchaseGameForm'">Purchase a Game</b-button>
          <royalePurchaseGameForm :yearQuarter="publisher.yearQuarter" :userRoyalePublisher = "publisher"></royalePurchaseGameForm>
        </div>
      </div>

      <h1>Games</h1>
      <b-table striped bordered small :items="publisher.publisherGames"></b-table>
    </div>
    
  </div>
</template>

<script>
  import Vue from "vue";
  import axios from "axios";

  import RoyalePurchaseGameForm from "components/modules/modals/royalePurchaseGameForm";

  export default {
    props: ['publisherID'],
    data() {
      return {
          errorInfo: "",
          publisher: null
      }
    },
    components: {
      RoyalePurchaseGameForm
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
