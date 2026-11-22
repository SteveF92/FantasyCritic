<template>
  <b-modal id="start-draft-modal" title="Warning!" @ok="startDraft">
    <p>Warning! Once you start drafting, you can no longer add or remove players, and the draft order is locked in.</p>
    <p>However, if you need to, you can reset the draft by pressing the "Reset Draft" link in the sidebar. That will let you change your players/settings again.</p>
  </b-modal>
</template>

<script lang="ts">
import { defineComponent } from 'vue';

export default defineComponent({
  emits: {
    draftStarted: () => true
  },
  methods: {
    startDraft(): void {
      this.$emit('draftStarted');
    }
  }
});
</script>
