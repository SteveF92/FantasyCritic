<template>
  <div role="tablist">
    <b-card no-body class="mb-1">
      <b-card-header header-tag="header" class="p-1" role="tab">
        <b-btn block href="#" v-b-toggle.accordion1 variant="primary">
          <font-awesome-icon icon="plus-circle" size="lg" transform="down-2" class="when-closed faq-icon" />
          <font-awesome-icon icon="minus-circle" size="lg" transform="down-2" class="when-opened faq-icon" />
          Why should I play FantasyCritic?
        </b-btn>
      </b-card-header>
      <b-collapse id="accordion1" visible accordion="my-accordion" role="tabpanel">
        <b-card-body>
          <p class="card-text">
            It’s a great catalyst for discussing video games with your friends, and gets everyone invested in checking out the year’s best video games.
          </p>
        </b-card-body>
      </b-collapse>
    </b-card>
    <b-card no-body class="mb-1">
      <b-card-header header-tag="header" class="p-1" role="tab">
        <b-btn block href="#" v-b-toggle.accordion2 variant="primary">
          <font-awesome-icon icon="plus-circle" size="lg" transform="down-2" class="when-closed faq-icon" />
          <font-awesome-icon icon="minus-circle" size="lg" transform="down-2" class="when-opened faq-icon" />
          Does this cost money?
        </b-btn>
      </b-card-header>
      <b-collapse id="accordion2" accordion="my-accordion" role="tabpanel">
        <b-card-body>
          <p class="card-text">
            No. It’s totally free. No catch. For now, this is a passion project for me and I’m comfortable eating the cost of servers and advertising.
            In the future, it is possible that I will consider adding advertising or opening a Patreon, but I’m never going to charge people to play.
          </p>
        </b-card-body>
      </b-collapse>
    </b-card>
    <b-card no-body class="mb-1">
      <b-card-header header-tag="header" class="p-1" role="tab">
        <b-btn block href="#" v-b-toggle.accordion3 variant="primary">
          <font-awesome-icon icon="plus-circle" size="lg" transform="down-2" class="when-closed faq-icon" />
          <font-awesome-icon icon="minus-circle" size="lg" transform="down-2" class="when-opened faq-icon" />
          How many players can be in a league?
        </b-btn>
      </b-card-header>
      <b-collapse id="accordion3" accordion="my-accordion" role="tabpanel">
        <b-card-body>
          <p class="card-text">
            Anywhere from 2 to 14. Somewhere between 4 and 8 is probably optimal, but you’ll do fine anywhere in that range.
          </p>
        </b-card-body>
      </b-collapse>
    </b-card>
    <b-card no-body class="mb-1">
      <b-card-header header-tag="header" class="p-1" role="tab">
        <b-btn block href="#" v-b-toggle.accordion4 variant="primary">
          <font-awesome-icon icon="plus-circle" size="lg" transform="down-2" class="when-closed faq-icon" />
          <font-awesome-icon icon="minus-circle" size="lg" transform="down-2" class="when-opened faq-icon" />
          How long does a league last?
        </b-btn>
      </b-card-header>
      <b-collapse id="accordion4" accordion="my-accordion" role="tabpanel">
        <b-card-body>
          <p class="card-text">
            Each calendar year is one “game” of Fantasy Critic. Everyone starts fresh with a new roster at the start of each new year,
            but you are free to keep playing in the same league with the same people year after year.
          </p>
        </b-card-body>
      </b-collapse>
    </b-card>
  </div>
</template>
<style scoped>
  .faq-icon{
    float: left;
  }

  .collapsed > .when-opened,
  :not(.collapsed) > .when-closed {
    display: none;
  }
</style>
