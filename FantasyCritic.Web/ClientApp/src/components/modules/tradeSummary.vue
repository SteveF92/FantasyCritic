<template>
  <div>
    <collapseCard>
      <div slot="header">Trade between {{trade.proposerPublisherName}} and {{trade.counterPartyPublisherName}} (Proposed on {{trade.proposedTimestamp | longDate }})</div>
      <div slot="body">
        <div class="row text-white">
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
      </div>
    </collapseCard>
  </div>
</template>

<script>
import MasterGamePopover from '@/components/modules/masterGamePopover';
import CollapseCard from '@/components/modules/collapseCard';

export default {
  props: ['trade', 'leagueYear', 'publisher'],
  components: {
    MasterGamePopover,
    CollapseCard
  },
  data() {
    return {

    };
  },
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
</style>
