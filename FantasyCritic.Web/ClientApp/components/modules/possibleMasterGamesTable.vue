<template>
    <table class="table">
        <thead>
          <tr>
            <th>Game Name</th>
            <th>Estimated Release Date</th>
            <th>Eligibility Level</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
            <tr v-for="game in possibleGames">
                <td>{{game.gameName}}</td>
                <td>{{game.estimatedReleaseDate}}</td>
                <td>
                  <eligibilityBadge :eligibilityLevel="game.eligibilityLevel" :maximumEligibilityLevel="maximumEligibilityLevel"></eligibilityBadge>
                </td>
                <td>
                    <b-button variant="info" v-on:click="selectGame(game)">Select</b-button>
                </td>
            </tr>
        </tbody>
    </table>
</template>
<script>
  import EligibilityBadge from "components/modules/eligibilityBadge";
    export default {
        data() {
            return {
                selectedMasterGame: null
            }
    },
    components: {
      EligibilityBadge
    },
    props: ['possibleGames', 'value', 'maximumEligibilityLevel'],
    methods: {
        selectGame(game) {
            this.selectedMasterGame = game;
            this.$emit('input', this.selectedMasterGame);
        }
    }
    }
</script>
