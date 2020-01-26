<template>
  <b-modal id="editAutoDraftForm" size="lg" ref="editAutoDraftFormRef" title="Edit Auto Draft">
    <div class="alert alert-info">
      If "Auto Draft" is turned on, the site will select your games for you when it is your turn.
      <br />
      Games will be chosen from your watchlist first, and if there are no available games on your watchlist, the available game with the highest hype factor will be chosen.
      <br />
      For counterpicks, the game with the highest counterpick % site-wide will be chosen.
    </div>
    <b-form inline>
      <b-form-checkbox class="mb-2 mr-sm-2 mb-sm-0" v-model="isAutoDraft">Auto Draft</b-form-checkbox>
    </b-form>
    <div slot="modal-footer">
      <input type="submit" class="btn btn-primary" value="Set Auto Draft" v-on:click="setAutoDraft"/>
    </div>
  </b-modal>
</template>
<script>
export default {
  data() {
      return {
        isAutoDraft: null
      }
    },
  props: ['publisher'],
  methods: {
    setAutoDraft() {
      // TODO: put in axois post logic
      alert('Auto Darft set');
    }
  },
  mounted() {
    this.isAutoDraft = this.publisher.autoDraft;
  }
}
</script>
