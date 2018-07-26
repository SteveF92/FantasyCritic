<template>
    <tr>
        <td>{{game.gameName}}</td>
        <td v-if="game.releaseDate">{{releaseDate}}</td>
        <td v-else>{{game.estimatedReleaseDate}} (Estimated)</td>
        <td>{{game.criticScore}}</td>
        <td>{{game.fantasyScore}}</td>
    </tr>
</template>
<script>
    import Vue from "vue";
    import moment from "moment";

    export default {
        props: ['game'],
        computed: {
            releaseDate() {
                return moment(this.game.releaseDate).format('MMMM Do, YYYY');
            }
        }
    }
</script>
