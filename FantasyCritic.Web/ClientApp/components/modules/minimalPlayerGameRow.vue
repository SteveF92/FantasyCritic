<template>
    <tr class="minimal-game-row">
        <td>{{game.gameName}}</td>
        <td class="score">{{game.criticScore | score}}</td>
        <td class="score">{{game.fantasyScore | score}}</td>
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
<style>
  .minimal-game-row td {
    font-size: 10pt;
  }
</style>
