<template>
  <div>
    <form class="form-horizontal" hide-footer>
      <b-modal id="proposeTradeForm" size="lg" ref="proposeTradeFormRef" title="Propose Trade" @hidden="clearData">
        <div class="alert alert-info">You can use this form to propose a trade.</div>
        <div class="form-group">
          <label for="counterParty" class="control-label">Publisher to trade with</label>
          <b-form-select v-model="counterParty">
            <option v-for="publisher in leagueYear.publishers" v-bind:value="publisher">
              {{ publisher.publisherName }}
            </option>
          </b-form-select>
        </div>

        <div slot="modal-footer">
          <input type="submit" class="btn btn-primary" value="Propose Trade" v-on:click="proposeTrade" :disabled="!tradeIsValid" />
        </div>
      </b-modal>
    </form>
  </div>
</template>
<script>
import Vue from 'vue';
import axios from 'axios';

export default {
  data() {
    return {
      counterParty: null,
      errorInfo: ''
    };
  },
  props: ['leagueYear', 'publisher'],
  computed: {
    tradeIsValid() {
      return true;
    }
  },
  methods: {
    proposeTrade() {
      var request = {

      };
      axios
        .post('/api/league/ProposeTrade', request)
        .then(response => {
          this.$refs.proposeTradeFormRef.hide();
          this.$emit('tradeProposed');
          this.clearData();
        })
        .catch(response => {
          this.errorInfo = response.response.data;
        });
    },
    clearData() {
      this.counterParty = null;
    }
  }
};
</script>
<style scoped>

  .add-game-button {
    width: 100%;
  }

  .claim-error {
    margin-top: 10px;
  }

  .game-search-input {
    margin-bottom: 15px;
  }

  .remove-error {
    margin-top: 15px;
  }
</style>
