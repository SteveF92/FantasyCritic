<template>
  <div class="tweet-area">
    <div class="tweet-timeline">
      <Timeline :id="'fantasy_critic'" :sourceType="'profile'" :options="{ theme: 'dark', tweetLimit: '3' }" />
    </div>
  </div>
</template>

<script>
  import { Timeline } from 'vue-tweet-embed'

  export default {
    components: {
      Timeline
    }
  }
</script>

<style scoped>
  .tweet-area {
    display: flex; /* establish flex container */
    justify-content: center; /* center items vertically, in this case */
    align-items: center; /* center items horizontally, in this case */
  }

  .tweet-timeline {
    width: 600px;
  }
</style>
