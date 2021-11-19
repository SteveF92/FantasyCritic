<template>
  <tr class="minimal-game-row" v-bind:class="{ 'counter-pick-row': counterPick }">
    <td class="game-column">
      <template v-if="counterPick">
        <span v-if="!yearFinished" class="empty-counterpick-warning">
          Warning!
          <font-awesome-icon color="white" size="lg" icon="info-circle" v-b-popover.hover="emptyCounterpickText" />
        </span>
        <span v-if="yearFinished" class="empty-counterpick-danger">
          Counter Pick Slot Not Filled
        </span>
      </template>
      
    </td>
    <td class="score-column">
      <template v-if="counterPick && yearFinished">-15</template>
    </td>
    <td class="score-column">
      <template v-if="counterPick && yearFinished">-15</template>
    </td>
  </tr>
</template>
<script>
  export default {
    props: ['counterPick', 'yearFinished'],
    computed: {
      emptyCounterpickText() {
        return {
          html: true,
          title: () => {
            return "Warning!";
          },
          content: () => {
            return 'If you do not fill this slot by the end of the year, it will count as -15 points. <br/> <br/>' +
              'See the FAQ for a full explanation.';
          }
        }
      }
    }
  };
</script>

<style scoped>
  .minimal-game-row td {
    font-size: 10pt;
  }

  .game-status {
    float: right;
    color: #B1B1B1;
    font-style: italic;
    padding-left: 5px;
  }

  .master-game-popover {
    float: left;
  }

  .counter-pick-row td {
    background-color: #AA1E1E !important;
  }

  .empty-counterpick-warning {
    float: right;
    color: #B1B1B1;
    font-style: italic;
    padding-left: 5px;
  }

  .empty-counterpick-danger {
    font-weight: bold;
  }
</style>
