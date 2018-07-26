<template>
    <div>
        <h3>Games</h3>
        <ul>
            <li v-for="game in games" v-if="!game.antiPick">
                {{ game.gameName }}
            </li>
        </ul>
        <h3>Anti Picks</h3>
        <ul>
            <li v-for="game in games" v-if="game.antiPick">
                {{ game.gameName }}
            </li>
        </ul>
    </div>
</template>
<script>
    import Vue from "vue";

    export default {
        props: ['games']
    }
</script>
