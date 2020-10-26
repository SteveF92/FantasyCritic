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
import axios from 'axios';

export default {
    data() {
        return {
            isAutoDraft: null
        };
    },
    props: ['publisher'],
    methods: {
        setAutoDraft() {
            var model = {
                publisherID: this.publisher.publisherID,
                autoDraft: this.isAutoDraft
            };
            axios
                .post('/api/league/setAutoDraft', model)
                .then(response => {
                    this.$refs.editAutoDraftFormRef.hide();
                    let actionInfo = {
                        autoDraft: this.isAutoDraft,
                        fetchLeagueYear: true
                    };
                    this.$emit('autoDraftSet', actionInfo);
                })
                .catch(response => {
                });
        }
    },
    mounted() {
        this.isAutoDraft = this.publisher.autoDraft;
    }
};
</script>
