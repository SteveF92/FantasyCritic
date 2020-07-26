<template>
  <b-modal id="eligibilityOverridesModal" ref="eligibilityOverridesModalRef" title="Eligibility Overrides" hide-footer>
    <table class="table table-sm table-responsive-sm table-bordered table-striped">
      <thead>
        <tr class="bg-primary">
          <th scope="col" class="game-column">Game</th>
          <th scope="col">Eligible?</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="eligibilityOverride in eligibilityOverrides">
          <td>{{eligibilityOverride.masterGame.gameName}}</td>
          <td>{{eligibilityOverride.eligible | yesNo}}</td>
        </tr>
      </tbody>
    </table>
  </b-modal>
</template>

<script>

    export default {
        props: ['eligibilityOverrides']
    }
</script>

