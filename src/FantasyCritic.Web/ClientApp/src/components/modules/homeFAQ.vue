<template>
  <div>
    <collapseCard defaultVisible>
      <div slot="header">
        Why should I play FantasyCritic?
      </div>
      <div slot="body">
        It’s a great catalyst for discussing video games with your friends, and gets everyone invested in checking out the year’s best video games.
      </div>
    </collapseCard>
    <collapseCard>
      <div slot="header">
        Does this cost money?
      </div>
      <div slot="body">
        Fantasy Critic is free to use. However, if you are able to, we would love it if you would support us on Patreon. Doing so will get you access to special
        rewards, including
        <router-link :to="{ name: 'fantasyCriticPlus' }" title="Fantasy Critic Plus">
          Fantasy Critic Plus.
        </router-link>
      </div>
    </collapseCard>
    <collapseCard>
      <div slot="header">
        How many players can be in a league?
      </div>
      <div slot="body">
        Anywhere from 2 to 20. Somewhere between 4 and 10 is probably optimal, and I'd be apprehensive about going over 14, but you can do it.
      </div>
    </collapseCard>
    <collapseCard>
      <div slot="header">
        How long does a league last?
      </div>
      <div slot="body">
        Each calendar year is one “game” of Fantasy Critic. Everyone starts fresh with a new roster at the start of each new year,
        but you are free to keep playing in the same league with the same people year after year.
      </div>
    </collapseCard>
  </div>
</template>

<script>
import CollapseCard from '@/components/modules/collapseCard';

export default {
  components: {
    CollapseCard
  }
};
</script>
