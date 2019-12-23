<template>
  <span>
    <span class="badge badge-pill badge-primary badge-warning" v-show="taken">Taken</span>
    <span class="badge badge-pill badge-primary badge-info" v-show="!taken && alreadyOwned">Already Owned</span>
    <span class="badge badge-pill badge-primary badge-success" v-show="!taken && !alreadyOwned && isEligible">Eligible</span>
    <span class="badge badge-pill badge-primary badge-warning" v-show="!taken && !alreadyOwned && !isEligible">Ineligible</span>
  </span>
</template>
<script>
  import Vue from "vue"

  export default {
    props: ['isEligible', 'taken', 'alreadyOwned']
  }
</script>
