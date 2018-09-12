<template>
    <div>
        <div class="playerTable">
            <table class="table table-bordered">
                <thead>
                    <tr>
                        <th scope="col" colspan="3">
                            {{publisher.publisherName}}
                            <br/>
                            Player: {{publisher.playerName}}
                        </th>
                    </tr>
                    <tr>
                        <th scope="col">Game</th>
                        <th scope="col">Critic</th>
                        <th scope="col">Points</th>
                    </tr>
                </thead>
                <tbody>
                    <minimalPlayerGameRow v-for="game in draftGames" :game="game"></minimalPlayerGameRow>
                    <minimalBlankPlayerGameRow v-for="blankSpace in draftFiller"></minimalBlankPlayerGameRow>
                    <minimalPlayerGameRow v-for="game in antiPicks" :game="game"></minimalPlayerGameRow>
                    <minimalBlankPlayerGameRow v-for="blankSpace in antiPickFiller"></minimalBlankPlayerGameRow>
                    <minimalPlayerGameRow v-for="game in waiverGames" :game="game"></minimalPlayerGameRow>
                    <minimalBlankPlayerGameRow v-for="blankSpace in waiverFiller"></minimalBlankPlayerGameRow>
                </tbody>
            </table>
        </div>
    </div>
</template>
<script>
    import Vue from "vue";
    import MinimalPlayerGameRow from "components/modules/minimalPlayerGameRow";
    import MinimalBlankPlayerGameRow from "components/modules/minimalBlankPlayerGameRow";

    export default {
        components: {
            MinimalPlayerGameRow,
            MinimalBlankPlayerGameRow
        },
        props: ['publisher', 'options'],
        computed: {
            games() {
                return this.publisher.games;
            },
            draftGames() {
                return _.filter(this.games, { 'antiPick': false, 'waiver': false });
            },
            antiPicks() {
                return _.filter(this.games, { 'antiPick': true });
            },
            waiverGames() {
                return _.filter(this.games, { 'waiver': true });
            },
            draftFiller() {
                var numberDrafted = this.draftGames.length;
                var openSlots = this.options.draftSlots - numberDrafted;
                return openSlots;
            },
            antiPickFiller() {
                var numberAntiPicked = this.antiPicks.length;
                var openSlots = this.options.antiPickSlots - numberAntiPicked;
                return openSlots;
            },
            waiverFiller() {
                var numberWaiverClaimed = this.waiverGames.length;
                var openSlots = this.options.waiverSlots - numberWaiverClaimed;
                return openSlots;
            }
        }
    }
</script>
<style scoped>
    .playerTable {
        margin-left: 3px;
        margin-right: 3px;
    }
</style>
<style>
  .playerTable table thead tr th {
    height: 35px;
    padding: 5px;
  } 
  .playerTable table tbody tr td {
    height: 35px;
    padding: 5px;
  }
  .playerTable table tbody tr td.score{
    text-align: center;
  } 
</style>
