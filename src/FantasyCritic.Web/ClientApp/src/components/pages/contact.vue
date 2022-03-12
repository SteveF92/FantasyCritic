<template>
    <div>
      <div class="col-md-10 offset-md-1 col-sm-12">
        <h1>Contact</h1>
        <hr />
        <h2>Adding new Games/Corrections</h2>
        <p class="text-well">
          <ul>
            <li>Please use the <router-link :to="{ name: 'masterGameRequest' }">Master Game Request form</router-link> for adding new games.</li>
            <li>Please use the "Suggest Correction" form, which can be accessed by clicking on a master game link and clicking the "Suggest a Correction" option for correcting existing games.</li>
            <li>If you have a particularly "contentious" question, like debating what tags a game should have, please bring those questions to #eligibility-debates in our <a href="https://discord.gg/dNa7DD3">Discord <font-awesome-icon icon="external-link-alt" size="sm" /></a>.</li>
          </ul>
          
          
        </p>
        <h2>Other questions/comments</h2>
        <p class="text-well">
          The best way to contact me is on Twitter <a href="https://twitter.com/fantasy_critic">@fantasy_critic <font-awesome-icon icon="external-link-alt" size="sm" /></a>,
          <a href="https://discord.gg/dNa7DD3">Discord <font-awesome-icon icon="external-link-alt" size="sm" /></a>,
          or on our <a href="https://www.reddit.com/r/fantasycritic/">subreddit <font-awesome-icon icon="external-link-alt" size="sm" /></a>.
        </p>
      </div>
    </div>
</template>

<script>
import Vue from 'vue';

export default {
  data() {
    return {
    };
  }
};
</script>
