<template>
  <b-card no-body class="mb-1">
    <b-card-header header-tag="header" class="p-1" role="tab">
      <div v-b-toggle="'accordion-' + _uid" class="btn btn-secondary faq-button">
        <font-awesome-icon icon="plus-circle" size="lg" transform="down-2" class="when-closed faq-icon" />
        <font-awesome-icon icon="minus-circle" size="lg" transform="down-2" class="when-opened faq-icon" />
        <slot name="header"></slot>
      </div>
    </b-card-header>
    <b-collapse :id="'accordion-' + _uid" v-model="visible" role="tabpanel">
      <b-card-body>
        <p class="card-text">
          <slot name="body"></slot>
        </p>
      </b-card-body>
    </b-collapse>
  </b-card>
</template>

<script>
export default {
  props: {
    defaultVisible: { type: Boolean }
  },
  data() {
    return {
      visible: this.defaultVisible
    };
  },
  methods: {
    show() {
      this.visible = true;
    }
  }
};
</script>

<style scoped>
.faq-icon {
  float: left;
}

.collapsed > .when-opened,
:not(.collapsed) > .when-closed {
  display: none;
}

.faq-button {
  width: 100%;
  cursor: pointer;
  font-weight: bold;
}
</style>
