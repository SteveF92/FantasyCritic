<template>
  <div>
    <faqCollapse :accordianID="faqAccordian" :collapseID="1" defaultVisible>
      <div slot="question">
        Why should I play FantasyCritic?
      </div>
      <div slot="answer">
        It’s a great catalyst for discussing video games with your friends, and gets everyone invested in checking out the year’s best video games.
      </div>
    </faqCollapse>
    <faqCollapse :accordianID="faqAccordian" :collapseID="2">
      <div slot="question">
        Does this cost money?
      </div>
      <div slot="answer">
        No. It’s totally free. No catch. For now, this is a passion project for me and I’m comfortable eating the cost of servers and advertising.
        In the future, it is possible that I will consider adding advertising or opening a Patreon, but I’m never going to charge people to play.
      </div>
    </faqCollapse>
    <faqCollapse :accordianID="faqAccordian" :collapseID="3">
      <div slot="question">
        How many players can be in a league?
      </div>
      <div slot="answer">
        Anywhere from 2 to 14. Somewhere between 4 and 8 is probably optimal, but you’ll do fine anywhere in that range.
      </div>
    </faqCollapse>
    <faqCollapse :accordianID="faqAccordian" :collapseID="4">
      <div slot="question">
        How long does a league last?
      </div>
      <div slot="answer">
        Each calendar year is one “game” of Fantasy Critic. Everyone starts fresh with a new roster at the start of each new year,
        but you are free to keep playing in the same league with the same people year after year.
      </div>
    </faqCollapse>
  </div>
</template>


<script>
  import FaqCollapse from "components/modules/faqCollapse";

  export default {
    components: {
      FaqCollapse
    }
  }
</script>
