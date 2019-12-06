<template>
  <b-modal id="dropGameForm" ref="dropGameFormRef" size="lg" title="Drop a Game" hide-footer @hidden="clearData">

  </b-modal>
</template>

<script>
  import Vue from "vue";
  import axios from "axios";

  export default {
    data() {
      return {
        dropResult: null,
        gameToDrop: null
      }
    },
    computed: {
      formIsValid() {
        return (this.dropMasterGame);
      }
    },
    props: ['publisher'],
    methods: {
      dropGame() {
        var request = {
            publisherID: this.leagueYear.userPublisher.publisherID,
            masterGameID: this.gameToDrop.masterGame.masterGameID
        };

        axios
          .post('/api/league/MakeDropRequest', request)
          .then(response => {
            this.dropResult = response.data;
            if (!this.dropResult.success) {
              return;
            }
            this.$refs.dropGameFormRef.hide();
            var dropInfo = {
              gameName: this.dropMasterGame.gameName,
            };
            this.$emit('dropRequestMade', dropInfo);
            this.clearData();
          })
          .catch(response => {

          });
      },
      clearData() {
        this.dropResult = null;
        this.gameToDrop = null;
      }
    }
  }
</script>
