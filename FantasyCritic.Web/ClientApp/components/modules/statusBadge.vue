<template>
  <span>
    <span class="badge badge-pill badge-primary badge-info" v-show="alreadyOwned">Already Owned</span>
    <span class="badge badge-pill badge-primary badge-danger" v-show="taken && !alreadyOwned">Taken</span>
    <span class="badge badge-pill badge-primary badge-success" v-show="!taken && isEligible">Eligible</span>
    <span class="badge badge-pill badge-primary badge-warning" v-show="!taken && !isEligible">Ineligible</span>
  </span>
</template>
<script>
  import Vue from "vue"

  export default {
    props: ['isEligible', 'taken', 'alreadyOwned']
  }
</script>
