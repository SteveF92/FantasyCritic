<template>
  <b-table :items="conferences" :fields="conferenceFields" thead-class="hidden_header" bordered striped>
    <template #cell(conferenceName)="data">
      <div class="row-flex">
        <router-link :to="{ name: 'conference', params: { conferenceid: data.item.conferenceID, year: data.item.activeYear } }" class="conference-icon">
          <font-awesome-icon icon="globe" size="2x" />
        </router-link>
        <div>
          <router-link :to="{ name: 'conference', params: { conferenceid: data.item.conferenceID, year: data.item.activeYear } }" class="conference-link">{{ data.item.conferenceName }}</router-link>
          <div class="manager">Manager: {{ data.item.conferenceManagerDisplayName }}</div>
        </div>
      </div>
    </template>
  </b-table>
</template>
<script>
export default {
  props: {
    conferences: { type: Array, required: true }
  },
  data() {
    return {
      conferenceFields: [{ key: 'conferenceName' }]
    };
  }
};
</script>
<style scoped>
table >>> .hidden_header {
  display: none;
}

.row-flex {
  display: flex;
}

.conference-icon {
  margin-top: 5px;
  margin-right: 17px;
  width: 40px;
  font-size: 20px;
}

.conference-link {
  font-weight: bold;
  margin-bottom: 0;
  padding-bottom: 0;
}

.manager {
  margin-top: 0;
  padding-top: 0;
  font-size: 14px;
}
</style>
