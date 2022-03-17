<template>
  <b-modal id="undoLastDraftActionModal" ref="undoLastDraftActionModalRef" title="Warning!" @ok="undoLastDraftAction">
    <p>
      Do you wish to undo the last draft action?
      <br />
      This will remove the game that was added most recently from it's publisher.
      <br />
      Only do this if there was a mistake.
    </p>
  </b-modal>
</template>
<script>
export default {
  methods: {
    undoLastDraftAction() {
      this.$refs.undoLastDraftActionModalRef.hide();
      this.$emit('undoLastDraftAction');
    }
  }
};
</script>
