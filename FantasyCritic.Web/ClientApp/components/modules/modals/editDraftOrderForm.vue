<template>
  <b-modal id="editDraftOrderForm" ref="editDraftOrderFormRef" title="Set Draft Order" size="lg" hide-footer>
  </b-modal>
</template>

<script>
    import Vue from "vue";
    import axios from "axios";

    export default {
      props: ['leagueYear'],
        data() {
          return {
            desiredDraftOrder: []
          }
      },
      methods: {
        setDesiredDraftOrder(sortedPublishers) {
          this.desiredDraftOrder = sortedPublishers.map(function (v) {
            return v.publisherID;
          });
        },
        setDraftOrder() {
          var model = {
            leagueID: this.leagueYear.leagueID,
            year: this.leagueYear.year,
            publisherDraftPositions: this.desiredDraftOrder
          };
          axios
            .post('/api/leagueManager/SetDraftOrder', model)
            .then(response => {
              this.$emit('draftOrderEdited');
            })
            .catch(response => {

            });
        }
      },
      mounted() {
        this.setDesiredDraftOrder(this.leagueYear.publishers);
      }
    }
</script>
<style scoped>
  .not-created-publisher {
    color: #B1B1B1;
    font-style: italic;
  }
</style>
