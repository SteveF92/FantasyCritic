<template>
    <div>
        <h2>Create a league</h2>
        <hr />
        <form method="post" class="form-horizontal" role="form" v-on:submit.prevent="postRequest">
            <div class="alert alert-danger" v-if="errorInfo">An error has occurred.</div>
            <div class="form-group col-md-10">
                <label for="leagueName" class="control-label">League Name</label>
                <input v-model="leagueName" id="leagueName" name="leagueName" type="text" class="form-control input" />
            </div>
            <hr />

            <div class="form-group col-md-10">
                <label for="draftGames" class="control-label">Number of Draft Games</label>
                <input v-model="draftGames" id="draftGames" name="draftGames" type="text" class="form-control input" />
            </div>
            <div class="form-group col-md-10">
                <label for="waiverGames" class="control-label">Number of Waiver Games</label>
                <input v-model="waiverGames" id="waiverGames" name="waiverGames" type="text" class="form-control input" />
            </div>
            <div class="form-group col-md-10">
                <label for="antiPicks" class="control-label">Number of Anti Picks</label>
                <input v-model="antiPicks" id="antiPicks" name="antiPicks" type="text" class="form-control input" />
            </div>
            <hr />

            <div class="form-group col-md-10">
                <label for="estimatedGameScore" class="control-label">Estimated Game Score</label>
                <input v-model="estimatedGameScore" id="estimatedGameScore" name="estimatedGameScore" type="text" class="form-control input" />
            </div>
            <div class="form-group col-md-10">
                <label for="eligibilitySystem" class="control-label">Eligibility System</label>
                <select class="form-control" v-model="eligibilitySystem" id="eligibilitySystem">
                    <option v-for="eligibilitySystem in leagueOptions.eligibilitySystems" v-bind:value="selectedEligibilitySystem">{{ eligibilitySystem }}</option>
                </select>
            </div>
            <div class="form-group col-md-10">
                <label for="draftSystem" class="control-label">Draft System</label>
                <select class="form-control" v-model="draftSystem" id="draftSystem">
                    <option v-for="draftSystem in leagueOptions.draftSystems" v-bind:value="selectedDraftSystem">{{ draftSystem }}</option>
                </select>
            </div>
            <div class="form-group col-md-10">
                <label for="waiverSystem" class="control-label">Waiver System</label>
                <select class="form-control" v-model="waiverSystem" id="waiverSystem">
                    <option v-for="waiverSystem in leagueOptions.waiverSystems" v-bind:value="selectedWaiverSystem">{{ waiverSystem }}</option>
                </select>
            </div>
            <div class="form-group col-md-10">
                <label for="scoringSystem" class="control-label">Scoring System</label>
                <select class="form-control" v-model="scoringSystem" id="scoringSystem">
                    <option v-for="scoringSystem in leagueOptions.scoringSystems" v-bind:value="selectedScoringSystem">{{ scoringSystem }}</option>
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
                leagueOptions: {},
                errorInfo: ""
            }
        },
        methods: {
            fetchLeagueOptions() {
                axios
                    .get('/api/League/LeagueOptions')
                    .then(response => {
                        this.leagueOptions = response.data;
                    })
                    .catch(returnedError => (this.error = returnedError));
            }
        },
        mounted() {
            this.fetchLeagueOptions();
        }
}
</script>
