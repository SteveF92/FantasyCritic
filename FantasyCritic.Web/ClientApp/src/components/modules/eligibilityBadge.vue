<template>
  <span class="badge badge-pill badge-primary"
        v-bind:class="{ 'badge-success': isEligible, 'badge-danger': !isEligible }">{{eligibilityLevel.name}}</span>
</template>
<script>
import Vue from 'vue';

export default {
    props: ['eligibilityLevel', 'maximumEligibilityLevel'],
    computed: {
        isEligible() {
            return (this.eligibilityLevel.level <= this.maximumEligibilityLevel.level);
        }
    }
};
</script>
