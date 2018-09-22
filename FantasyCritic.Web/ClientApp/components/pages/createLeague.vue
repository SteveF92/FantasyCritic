<template>
    <div>
        <h2>Create a league</h2>
        <hr />
        <form method="post" class="form-horizontal" role="form" v-on:submit.prevent="postRequest">
            <div class="alert alert-danger" v-if="errorInfo">An error has occurred.</div>
            <div class="form-group col-md-10">
                <label for="leagueName" class="control-label">League Name</label>
                <input v-model="selectedLeagueOptions.leagueName" id="leagueName" name="leagueName" type="text" class="form-control input" />
            </div>
            <hr />

            <div class="form-group col-md-10">
                <label for="draftGames" class="control-label">Number of Draft Games</label>
                <input v-model="selectedLeagueOptions.draftGames" id="draftGames" name="draftGames" type="text" class="form-control input" />
            </div>
            <div class="form-group col-md-10">
                <label for="waiverGames" class="control-label">Number of Waiver Games</label>
                <input v-model="selectedLeagueOptions.waiverGames" id="waiverGames" name="waiverGames" type="text" class="form-control input" />
            </div>
            <div class="form-group col-md-10">
                <label for="counterPicks" class="control-label">Number of Counter Picks</label>
                <input v-model="selectedLeagueOptions.counterPicks" id="counterPicks" name="counterPicks" type="text" class="form-control input" />
            </div>
            <hr />

            <div class="form-group col-md-10">
                <label for="estimatedCriticScore" class="control-label">Estimated Game Score</label>
                <input v-model="selectedLeagueOptions.estimatedCriticScore" id="estimatedCriticScore" name="estimatedCriticScore" type="text" class="form-control input" />
            </div>
            <div class="form-group col-md-10">
                <label for="intialYear" class="control-label">Year to Play</label>
                <select class="form-control" v-model="selectedLeagueOptions.initialYear" id="initialYear">
                    <option v-for="initialYear in possibleLeagueOptions.openYears" v-bind:value="initialYear">{{ initialYear }}</option>
                </select>
            </div>
            <hr />

            <div class="form-group col-md-10">
                <label for="maximumEligibilityLevel" class="control-label">Maximum Eligibility Level</label>
                <input v-model="selectedLeagueOptions.maximumEligibilityLevel" id="maximumEligibilityLevel" name="maximumEligibilityLevel" type="text" class="form-control input" />
            </div>
            <div class="form-group col-md-10">
                <label for="draftSystem" class="control-label">Draft System</label>
                <select class="form-control" v-model="selectedLeagueOptions.draftSystem" id="draftSystem">
                    <option v-for="draftSystem in possibleLeagueOptions.draftSystems" v-bind:value="draftSystem">{{ draftSystem }}</option>
                </select>
            </div>
            <div class="form-group col-md-10">
                <label for="waiverSystem" class="control-label">Waiver System</label>
                <select class="form-control" v-model="selectedLeagueOptions.waiverSystem" id="waiverSystem">
                    <option v-for="waiverSystem in possibleLeagueOptions.waiverSystems" v-bind:value="waiverSystem">{{ waiverSystem }}</option>
                </select>
            </div>
            <div class="form-group col-md-10">
                <label for="scoringSystem" class="control-label">Scoring System</label>
                <select class="form-control" v-model="selectedLeagueOptions.scoringSystem" id="scoringSystem">
                    <option v-for="scoringSystem in possibleLeagueOptions.scoringSystems" v-bind:value="scoringSystem">{{ scoringSystem }}</option>
                </select>
            </div>

            <div class="form-group">
                <div class="col-md-offset-2 col-md-10">
                    <input type="submit" class="btn btn-primary" value="Create League" />
                </div>
            </div>
        </form>
    </div>
</template>
<script>
import Vue from "vue";
import axios from "axios";
export default {
        data() {
            return {
                errorInfo: "",
                possibleLeagueOptions: {},
                selectedLeagueOptions: {
                    leagueName: "",
                    draftGames: "",
                    waiverGames: "",
                    counterPicks: "",
                    estimatedCriticScore: "",
                    initialYear: "",
                    maximumEligibilityLevel: "",
                    draftSystem: "",
                    waiverSystem: "",
                    scoringSystem: ""
                }
            }
        },
        methods: {
            fetchLeagueOptions() {
                axios
                    .get('/api/League/LeagueOptions')
                    .then(response => {
                        this.possibleLeagueOptions = response.data;
                    })
                    .catch(returnedError => (this.error = returnedError));
            },
            postRequest() {
                axios
                    .post('/api/league/createLeague', this.selectedLeagueOptions)
                    .then(this.responseHandler)
                    .catch(this.catchHandler);
            },
            responseHandler(response) {
                this.$router.push({ name: "home" });
            },
            catchHandler(returnedError) {
                this.errorInfo = returnedError;
            }
        },
        mounted() {
            this.fetchLeagueOptions();
        }
}
</script>
