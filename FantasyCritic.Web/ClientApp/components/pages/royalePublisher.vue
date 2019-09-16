<template>
  <div class="col-md-10 offset-md-1 col-sm-12">
    <div v-if="publisher">
      <h1>{{publisher.publisherName}}</h1>

      <div class="text-well top-area">
        <div class="row main-buttons">
          <b-button variant="primary" class="main-button">Add a Game</b-button>
          <b-button variant="warning" class="main-button">Sell a Game</b-button>
        </div>
      </div>
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
