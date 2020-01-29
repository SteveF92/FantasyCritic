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
      <b-form-checkbox-group
        v-model="selected"
        :options="options"
        name="audoDraft"
        stacked
      ></b-form-checkbox-group>
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
  data: () => ({ selected: [] }),
  computed: {
    options() {
      return this.leagueYear.publishers.map(pub => ({
        text: pub.publisherName,
        value: pub.publisherID
      }))
    }
  },
  methods: {
    setAutoDraft() {
      if (!(this.selected.length > 0)) return

      const publisherAudoDraft = this.selected.map(publisherID => ({
        publisherID,
        autoDraft: true
      }));
      const model = {
        leagueID: this.leagueYear.leagueID,
        year: this.leagueYear.year,
        publisherAudoDraft
      };
      axios
        .post('/api/leagueManager/SetAutoDraft', model)
        .then(response => {
          this.$refs.managerSetAutoDraftForm.hide();
          let actionInfo = {
            autoDraft: this.isAutoDraft,
            fetchLeagueYear: true
          };
          this.$emit('publishersAutoDraftSet');
        })
        .catch(e => {});
    }
  }
}
</script>
