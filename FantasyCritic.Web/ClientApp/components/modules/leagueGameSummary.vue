<template>
  <div>
    <div class="row" v-for="i in Math.ceil(publishers.length / 2)">
      <span v-for="publisher in publishers.slice((i - 1) * 2, i * 2)" class="minimalPlayerTable">
        <minimalPlayerGameTable :publisher="publisher" :options="options"></minimalPlayerGameTable>
      </span>
    </div>
  </div>
</template>
<script>
    import Vue from "vue";
    import MinimalPlayerGameTable from "components/modules/minimalPlayerGameTable";

    export default {
        components: {
            MinimalPlayerGameTable
        },
        props: ['leagueYear'],
        computed: {
            publishers() {
                return this.leagueYear.publishers;
            },
            options() {
                var options = {
                    draftSlots: this.leagueYear.draftGames,
                    antiPickSlots: this.leagueYear.antiPicks,
                    waiverSlots: this.leagueYear.waiverGames
                };

                return options;
            }
        }
    }
</script>
<style>
    .minimalPlayerTable {
        display: inline-block;
        width: 50%;
    }
</style>
