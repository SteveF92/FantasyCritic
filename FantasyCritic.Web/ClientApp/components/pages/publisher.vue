<template>
    <div v-if="publisher">
        <h2>{{publisher.publisherName}}</h2>
        <h4>{{publisher.playerName}}</h4>
        <h5>
            <router-link :to="{ name: 'league', params: { leagueid: publisher.leagueID }}">League: {{publisher.leagueName}}</router-link>
        </h5>
        <playerGameTable :games="publisher.games"></playerGameTable>
    </div>
</template>

<script>
    import Vue from "vue";
    import axios from "axios";
    import PlayerGameTable from "components/modules/playerGameTable";

    export default {
        data() {
            return {
                errorInfo: "",
                publisher: null
            }
        },
        components: {
            PlayerGameTable
        },
        props: ['publisherid'],
        methods: {
            fetchPublisher() {
                axios
                    .get('/api/League/GetPublisher/' + this.publisherid)
                    .then(response => {
                        this.publisher = response.data;
                    })
                    .catch(returnedError => (this.error = returnedError));
            }
        },
        mounted() {
            this.fetchPublisher();
        },
        watch: {
            '$route'(to, from) {
                this.fetchPublisher();
            }
        }
    }
</script>
