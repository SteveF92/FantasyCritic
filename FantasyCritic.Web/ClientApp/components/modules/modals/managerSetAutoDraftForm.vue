<template>
  <b-modal
    id="managerSetAutoDraftForm"
    size="lg"
    ref="managerSetAutoDraftFormRef"
    title="Edit Auto Draft"
  >
    <div class="alert alert-info">
      Description
    </div>

    <b-form-group label="Form-checkbox-group stacked checkboxes">
      <b-form-checkbox
        v-for="publisher in publishers"
        v-model="publisher.autoDraft"
        @change="onChange(publisher)"
      >
        {{ publisher.publisherName }}
      </b-form-checkbox>
    </b-form-group>

    <div slot="modal-footer">
      <input
        type="submit"
        class="btn btn-primary"
        value="Set Auto Draft"
        v-on:click="setAutoDraft"
      />
    </div>
  </b-modal>
</template>
<script>
import axios from 'axios'

export default {
  name: 'managerSetAutoDraftForm',
  props: ['leagueYear'],
  data: () => ({ publishers: [], publisherAutoDraft: {} }),
  methods: {
    // Keep track of publishers that need the updates.
    onChange(pub) {
      this.publisherAutoDraft[pub.publisherID] = !pub.autoDraft
    },
    setAutoDraft() {
      if (Object.keys(this.publisherAutoDraft).length === 0) return

      const model = {
        leagueID: this.leagueYear.leagueID,
        year: this.leagueYear.year,
        publisherAutoDraft: this.publisherAutoDraft
      };
      axios
        .post('/api/leagueManager/SetAutoDraft', model)
        .then(response => {
          this.$refs.managerSetAutoDraftForm.hide();
          this.$emit('publishersAutoDraftSet');
        })
        .catch(e => {});
    }
  },
  mounted() {
    this.publishers = this.leagueYear.publishers
  }
}
</script>
