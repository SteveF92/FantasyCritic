<template>
    <div>
        <table class="table">
            <thead>
                <tr>
                    <th scope="col">Game Name</th>
                    <th scope="col">Release Date</th>
                    <th scope="col">Critic Score</th>
                    <th scope="col">Fantasy Score</th>
                </tr>
            </thead>
            <tbody>
                <playerGameRow v-for="game in games" :game="game" v-if="!game.counterPick && !game.waiver"></playerGameRow>
                <playerGameRow v-for="game in games" :game="game" v-if="game.waiver"></playerGameRow>
                <playerGameRow v-for="game in games" :game="game" v-if="game.counterPick"></playerGameRow>
            </tbody>
        </table>
    </div>
</template>
<script>
    import Vue from "vue";
    import PlayerGameRow from "components/modules/playerGameRow";

    export default {
        components: {
            PlayerGameRow
        },
        props: ['games']
    }
</script>
