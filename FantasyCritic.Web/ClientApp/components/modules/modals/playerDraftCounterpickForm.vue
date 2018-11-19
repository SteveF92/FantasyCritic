<template>
  <b-modal id="playerDraftGameForm" ref="playerDraftGameFormRef" title="Select Draft Game" hide-footer @hidden="clearData">
    <form class="form-horizontal" v-on:submit.prevent="selectCounterPick" hide-footer>
        <div class="form-group">
            <label for="selectedCounterPick" class="control-label">Game</label>
            <b-form-select v-model="selectedCounterPick">
              <option v-for="publisherGame in availableCounterPicks" v-bind:value="publisherGame">
                {{ publisherGame.gameName }}
              </option>
            </b-form-select>
        </div>

        <div v-if="selectedCounterPick">
          <input type="submit" class="btn btn-primary add-game-button" value="Select Game as Counter-Pick" />
        </div>
    </form>
  </b-modal>
</template>

<script>
    import Vue from "vue";
    import axios from "axios";
    export default {
        data() {
            return {
              selectedCounterPick: null
            }
        },
        props: ['userPublisher', 'availableCounterpicks'],
        methods: {
            selectCounterPick() {
              var request = {
                publisherID: this.userPublisher.publisherID,
                gameName: this.selectedCounterPick.gameName,
                counterPick: this.draftCounterPick,
                masterGameID: this.selectedCounterPick.masterGameID,
              };

              axios
                .post('/api/league/DraftGame', request)
                .then(response => {
                  this.draftResult = response.data;
                  if (!this.draftResult.success) {
                    return;
                  }
                  this.$refs.playerDraftGameFormRef.hide();
                  var draftInfo = {
                    gameName
                  };
                  this.$emit('counterpickDrafted', draftInfo);
                  this.selectedCounterPick = null;
                })
                .catch(response => {

                });
            },
            clearData() {
              this.selectedCounterPick = null;
            }
        }
    }
</script>
<style scoped>
.add-game-button{
  width: 100%;
}
</style>
