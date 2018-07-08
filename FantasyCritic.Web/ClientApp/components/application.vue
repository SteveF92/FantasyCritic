<template>
    <div id="vue-app">
        <div id="top-nav-bar">
            <top-navbar></top-navbar>
        </div>
        <div class="container-fluid">
            <div class="row">
                <main class="col-12">
                    <router-view></router-view>
                </main>
            </div>
        </div>
    </div>
</template>

<script>
    import TopNavbar from "components/modules/shared/topNavbar";

    export default {
        components: {
            TopNavbar
        }
    };
</script>

<style>
</style>
