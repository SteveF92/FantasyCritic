<template>
  <span>
    <span class="badge badge-pill badge-primary badge-info" v-show="alreadyOwned">Already Owned</span>
    <span class="badge badge-pill badge-primary badge-success" v-show="!alreadyOwned && isEligible">Eligible</span>
    <span class="badge badge-pill badge-primary badge-warning" v-show="!alreadyOwned && !isEligible">Ineligible</span>
  </span>
</template>
<script>
  import Vue from "vue"

  export default {
    props: ['isEligible', 'taken', 'alreadyOwned']
  }
</script>
