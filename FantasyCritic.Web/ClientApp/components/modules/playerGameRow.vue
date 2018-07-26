<template>
    <tr>
        <td>{{game.gameName}}</td>
        <td v-if="game.releaseDate">{{game.releaseDate}}</td>
        <td v-else>{{game.estimatedReleaseDate}} (Estimated)</td>
        <td>{{game.criticScore}}</td>
        <td>{{game.fantasyScore}}</td>
    </tr>
</template>
<script>
    import Vue from "vue";

    export default {
        props: ['game']
    }
</script>
