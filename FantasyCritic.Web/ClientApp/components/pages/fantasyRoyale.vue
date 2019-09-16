<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <h1>Fantasy Royale</h1>
      <b-button variant="primary" :to="{ name: 'createRoyalePublisher' }" class="main-button">Start playing!</b-button>
      <h2>Leaderboards {{year}}-Q{{quarter}}</h2>
    </div>
  </div>
</template>

<script>
    import Vue from 'vue';

  export default {
      props: ['year', 'quarter'],
      data() {
          return {
          }
      }
    }
</script>
