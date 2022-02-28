<template>
  <div>
    <collapseCard :defaultVisible="defaultVisible">
      <div slot="header">Trade between {{trade.proposerPublisherName}} and {{trade.counterPartyPublisherName}} (Proposed on {{trade.proposedTimestamp | longDate }})</div>
      <div slot="body">
        <div class="row">
          <div class="col-6">
            <h4>{{trade.proposerPublisherName}}</h4>
            <h5>{{trade.proposerDisplayName}}</h5>
            <h5>Receives</h5>
            <div v-for="game in trade.counterPartySendGames" class="component-row">
              • <masterGamePopover :masterGame="game.masterGameYear"></masterGamePopover>
            </div>
            <div v-if="trade.counterPartyBudgetSendAmount" class="component-row">
              • ${{trade.counterPartyBudgetSendAmount}} of budget
            </div>
          </div>
          <div class="col-6">
            <h4>{{trade.counterPartyPublisherName}}</h4>
            <h5>Player: {{trade.counterPartyDisplayName}}</h5>
            <h5>Receives</h5>
            <div v-for="game in trade.proposerSendGames" class="component-row">
              • <masterGamePopover :masterGame="game.masterGameYear"></masterGamePopover>
            </div>
            <div v-if="trade.proposerBudgetSendAmount" class="component-row">
              • ${{trade.proposerBudgetSendAmount}} of budget
            </div>
          </div>
        </div>
        <div class="trade-status-list">
          <div>• Trade proposed by {{trade.proposerPublisherName}} at {{trade.proposedTimestamp | dateTime}}</div>
          <div v-if="trade.status === 'Proposed' && !isCounterParty">• Trade is waiting for approval from {{trade.counterPartyPublisherName}}</div>
          <div class="alert alert-info" v-if="trade.status === 'Proposed' && isCounterParty">
            This trade is waiting for your approval.
            <b-button variant="success" v-on:click="acceptTrade">Accept</b-button>
            <b-button variant="danger" v-on:click="rejectTrade">Reject</b-button>
          </div>
        </div>
      </div>
    </collapseCard>
  </div>
</template>

<script>
import axios from 'axios';
import MasterGamePopover from '@/components/modules/masterGamePopover';
import CollapseCard from '@/components/modules/collapseCard';

export default {
  props: ['trade', 'leagueYear', 'publisher', 'defaultVisible'],
  components: {
    MasterGamePopover,
    CollapseCard
  },
  data() {
    return {

    };
  },
  computed: {
    isProposer() {
      return this.trade.proposerPublisherID === this.publisher.publisherID;
    },
    isCounterParty() {
      return this.trade.counterPartyPublisherID === this.publisher.publisherID;
    }
  },
  methods: {
    acceptTrade() {
      this.sendGenericTradeRequest('AcceptTrade');
    },
    rejectTrade() {
      this.sendGenericTradeRequest('RejectTrade');
    },
    rescindTrade() {
      this.sendGenericTradeRequest('RescindTrade');
    },
    sendGenericTradeRequest(endPoint) {
      var model = {
        tradeID: this.trade.tradeID
      };
      axios
        .post(`/api/league/${endPoint}`, model)
        .then(response => {
          this.$emit('tradeActioned');
        })
        .catch(response => {

        });
    }
  }
};
</script>
<style scoped>
  .component-row {
    width: 100%;
    background-color: #555555;
    margin-bottom: 3px;
    padding: 5px;
    border-radius: 10px;
  }

  div, p {
    color: white;
  }

  .trade-status-list div {
      margin-bottom: 10px;
  }
</style>
