<template>
  <div>
    <div v-for="masterGameYear in reversedMasterGameYears" :key="masterGameYear.year" class="text-well">
      <h2>Stats for {{ masterGameYear.year }}</h2>
      <ul>
        <li>Drafted or picked up in {{ masterGameYear.eligiblePercentStandardGame | percent(1) }} of leagues where it is eligible.</li>

        <li v-if="masterGameYear.averageDraftPosition">Average Draft Position: {{ masterGameYear.averageDraftPosition | score(1) }}</li>
        <li v-else>Average Draft Position: Undrafted</li>

        <li v-if="masterGameYear.dateAdjustedHypeFactor">Hype Factor: {{ masterGameYear.dateAdjustedHypeFactor | score(1) }}</li>
        <li v-else>Hype Factor: Unhyped...</li>

        <template v-if="masterGameYear.year >= 2022 && masterGameYear.peakHypeFactor > masterGameYear.dateAdjustedHypeFactor">
          <li v-if="masterGameYear.peakHypeFactor">
            Peak Hype Factor: {{ masterGameYear.peakHypeFactor | score(1) }}
            <font-awesome-icon v-b-popover.hover.top="peakHypeFactorText" color="white" size="lg" icon="info-circle" />
          </li>
          <li v-else>
            Peak Hype Factor: Unhyped...
            <font-awesome-icon v-b-popover.hover.top="peakHypeFactorText" color="white" icon="info-circle" />
          </li>
        </template>

        <li v-if="masterGameYear.projectedFantasyPoints">Projected Points: ~{{ masterGameYear.projectedFantasyPoints | score(1) }}</li>

        <li>Counter Picked in {{ masterGameYear.adjustedPercentCounterPick | percent(1) }} of leagues where it is published.</li>
      </ul>
    </div>
  </div>
</template>

<script>
export default {
  props: {
    masterGameYears: { type: Array, required: true }
  },
  data() {
    return {};
  },
  computed: {
    reversedMasterGameYears() {
      let tempMasterGameYears = structuredClone(this.masterGameYears);
      tempMasterGameYears.reverse();
      return tempMasterGameYears;
    },
    peakHypeFactorText() {
      return {
        html: true,
        title: () => {
          return 'Peak Hype Factor';
        },
        content: () => {
          return (
            "Sometimes a game's hype factor will go down over the course of the year, particularly if it gets delayed and many players drop it. " +
            "This number is the highest this game's hype factor ever was in the year."
          );
        }
      };
    }
  }
};
</script>
