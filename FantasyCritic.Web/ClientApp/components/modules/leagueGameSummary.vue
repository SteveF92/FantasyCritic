<template>
    <div>
        <minimalPlayerGameTable v-for="publisher in publishers" :publisher="publisher" :options="options" class="minimalPlayerTable"></minimalPlayerGameTable>
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
    }
</style>
