<template>
  <b-modal id="editAudoDraftForm" ref="editAudoDraftFormRef" title="Edit Audo Draft">
    <b-form inline>
      <b-form-checkbox class="mb-2 mr-sm-2 mb-sm-0" v-model="isAudoDraft">Auto Draft</b-form-checkbox>
    </b-form>
    <!-- Question: Needed? Checkbox should be enough right -->
    <div slot="modal-footer">
      <input type="submit" class="btn btn-primary" value="Set Auto Draft" v-on:click="setDraftOrder" />
    </div>
  </b-modal>
</template>
<script>

  export default {
    name: 'editAudoDraftForm',
    data:() => ({ isAudoDraft }),
    props: ['publisher'],
    methods: {
      setDraftOrder() {
        // TODO: put in axois post logic
        alert('Audo Darft set')
      }
    },
    mounted() {
      this.isAudoDraft = publisher.audoDraft
    }
  }
</script>
