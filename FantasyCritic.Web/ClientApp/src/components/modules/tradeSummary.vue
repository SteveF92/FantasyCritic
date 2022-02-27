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
          </div>
          <div class="col-6">
            <h4>{{trade.counterPartyPublisherName}}</h4>
            <h5>Player: {{trade.counterPartyDisplayName}}</h5>
            <h5>Receives</h5>
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
