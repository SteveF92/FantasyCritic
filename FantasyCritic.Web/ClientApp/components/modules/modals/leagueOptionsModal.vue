<template>
  <b-modal id="leagueOptionsModal" ref="leagueOptionsModalRef" title="League Options" hide-footer>
    <div v-if="leagueYear">
      <table class="table table-responsive-sm table-bordered table-striped">
        <tbody>
          <tr>
            <th class="bg-primary">Total Standard Games</th>
            <td>10</td>
          </tr>
          <tr>
            <th class="bg-primary">Games to Draft</th>
            <td>10</td>
          </tr>
          <tr>
            <th class="bg-primary">Pickup Games</th>
            <td>10</td>
          </tr>
          <tr>
            <th class="bg-primary">Counter Picks</th>
            <td>10</td>
          </tr>
          <tr>
            <th class="bg-primary">Maximum Eligibility Level</th>
            <td>10</td>
          </tr>
          <tr>
            <th class="bg-primary">Allow Yearly Installments</th>
            <td>10</td>
          </tr>
          <tr>
            <th class="bg-primary">Allow Early Release</th>
            <td>10</td>
          </tr>

          <tr>
            <th class="bg-primary">Public League</th>
            <td>10</td>
          </tr>
          <tr>
            <th class="bg-primary">Test League</th>
            <td>10</td>
          </tr>
        </tbody>
      </table>
    </div>
  </b-modal>
</template>

<script>
    export default {
        props: ['leagueYear']
    }
</script>
<style scoped>

</style>
