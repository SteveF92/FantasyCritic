<template>
  <div class="tweet-area">
    <div class="tweet-timeline">
      <TwitterTimeline url="fantasy_critic" description="Loading Tweets..." theme="dark" doNotTrack></TwitterTimeline>
    </div>
  </div>
</template>

<script setup>
import TwitterTimeline from 'vue-twitter-timeline';
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
