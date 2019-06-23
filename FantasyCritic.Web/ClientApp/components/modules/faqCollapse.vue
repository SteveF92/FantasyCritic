<template>
  <b-card no-body class="mb-1">
    <b-card-header header-tag="header" class="p-1" role="tab">
      <b-btn block href="#" v-b-toggle="'accordion-' + collapseID" variant="primary">
        <font-awesome-icon icon="plus-circle" size="lg" transform="down-2" class="when-closed faq-icon" />
        <font-awesome-icon icon="minus-circle" size="lg" transform="down-2" class="when-opened faq-icon" />
        <slot name="question"></slot>
      </b-btn>
    </b-card-header>
    <b-collapse :id="'accordion-' + collapseID" :visible="defaultVisible" :accordion="accordionID" role="tabpanel">
      <b-card-body>
        <p class="card-text">
          <slot name="answer"></slot>
        </p>
      </b-card-body>
    </b-collapse>
  </b-card>
</template>

<script>
  export default {
    props: ['accordionID','collapseID', 'defaultVisible']
  }
</script>

<style scoped>
  .faq-icon {
    float: left;
  }

  .collapsed > .when-opened,
  :not(.collapsed) > .when-closed {
    display: none;
  }
</style>
