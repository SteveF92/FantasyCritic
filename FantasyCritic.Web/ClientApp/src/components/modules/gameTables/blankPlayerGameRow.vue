<template>
  <tr v-bind:class="{ 'table-danger': counterPick }">
    <td>
      <span v-if="counterPick" class="empty-counterpick-warning">
        Warning!
        <font-awesome-icon color="white" size="lg" icon="info-circle" v-b-popover.hover="emptyCounterpickText" />
      </span>
    </td>
    <td></td>
    <td></td>
    <td><template v-if="counterPick && yearFinished">-15</template></td>
    <td><template v-if="counterPick && yearFinished">-15</template></td>
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
  tr {
    height: 40px;
  }

  .empty-counterpick-warning {
    float: right;
    color: #B1B1B1;
    font-style: italic;
    padding-left: 5px;
  }
</style>
