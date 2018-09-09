<template>
    <table class="table">
        <thead>
            <tr>
                <th>Game Name</th>
                <th>Estimated Release Date</th>
                <th></th>
            </tr>
        </thead>
        <tbody>
            <tr v-for="game in possibleGames">
                <td>{{game.gameName}}</td>
                <td>{{game.estimatedReleaseDate}}</td>
                <td>
                    <b-button variant="info" v-on:click="selectGame(game)">Select</b-button>
                </td>
            </tr>
        </tbody>
    </table>
</template>
<script>
    export default {
        data() {
            return {
                selectedMasterGame: null
            }
        },
        props: ['possibleGames', 'value'],
        methods: {
            selectGame(game) {
                this.selectedMasterGame = game;
                this.$emit('input', this.selectedMasterGame);
            }
        }
    }
</script>
