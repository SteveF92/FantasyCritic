<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <div v-if="hasError" class="alert alert-danger" role="alert">
        Something went wrong with this conference. Contact us on Twitter or Discord for support. Please include the conference ID in your message (Linking the URL will do).
      </div>
      <div v-if="errorInfo" class="alert alert-danger" role="alert">{{ errorInfo }}</div>
      <div v-if="conferenceYear">
        <h1>{{ conference.conferenceName }}</h1>
      </div>
    </div>
  </div>
</template>
<script>
import { mapGetters } from 'vuex';

export default {
  props: {
    conferenceid: { type: String, required: true },
    year: { type: Number, required: true }
  },
  data() {
    return {
      selectedYear: null,
      errorInfo: null
    };
  },
  computed: {
    ...mapGetters(['conference', 'conferenceYear', 'hasError'])
  },
  watch: {
    async $route(to, from) {
      if (to.path !== from.path) {
        await this.initializePage();
      }
    }
  },
  async created() {
    await this.initializePage();
  },
  methods: {
    async initializePage() {
      this.selectedYear = this.year;
      const conferencePageParams = { conferenceID: this.conferenceid, year: this.year };
      await this.$store.dispatch('initializeConferencePage', conferencePageParams);
    }
  }
};
</script>
