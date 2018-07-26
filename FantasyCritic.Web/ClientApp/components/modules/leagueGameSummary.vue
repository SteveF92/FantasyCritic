<template>
    <div>
        <minimalPlayerGameTable v-for="player in players" :playerName="player.userName" :games="player.games" class="minimalPlayerTable"></minimalPlayerGameTable>
    </div>
</template>
<script>
    import Vue from "vue";
    import MinimalPlayerGameTable from "components/modules/minimalPlayerGameTable";

    export default {
        components: {
            MinimalPlayerGameTable
        },
        props: ['players']
    }
</script>
<style>
    .minimalPlayerTable {
        display: inline-block;
    }
</style>
