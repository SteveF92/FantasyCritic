<template>
    <div>
        <div class="playerTable">
            <table class="table table-bordered">
                <thead>
                    <tr>
                        <th scope="col" colspan="3">{{playerName}}</th>
                    </tr>
                    <tr>
                        <th scope="col">Game</th>
                        <th scope="col">Critic</th>
                        <th scope="col">Fantasy</th>
                    </tr>
                </thead>
                <tbody>
                    <minimalPlayerGameRow v-for="game in games" :game="game" v-if="!game.antiPick && !game.waiver"></minimalPlayerGameRow>
                    <minimalPlayerGameRow v-for="game in games" :game="game" v-if="game.waiver"></minimalPlayerGameRow>
                    <minimalPlayerGameRow v-for="game in games" :game="game" v-if="game.antiPick"></minimalPlayerGameRow>
                </tbody>
            </table>
        </div>
    </div>
</template>
<script>
    import Vue from "vue";
    import MinimalPlayerGameRow from "components/modules/minimalPlayerGameRow";

    export default {
        components: {
            MinimalPlayerGameRow
        },
        props: ['playerName', 'games']
    }
</script>
<style scoped>
    .playerTable {
        margin-left: 3px;
        margin-right: 3px;
    }
</style>
