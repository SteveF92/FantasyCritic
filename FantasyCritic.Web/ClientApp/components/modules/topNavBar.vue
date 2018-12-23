<template>
    <div>
      <nav class="navbar navbar-expand navbar-light bg-white">
        <a class="navbar-brand" href="#">Navbar</a>
        <div class="navbar-collapse collapse">
          <ul class="navbar-nav">
            <li class="nav-item">
              <a class="nav-link" href="#">Link</a>
            </li>
            <li class="nav-item">
              <a class="nav-link" href="#">Link</a>
            </li>
            <li class="nav-item">
              <a class="nav-link" href="#">Link</a>
            </li>
          </ul>
        </div>
        <div class="my-2 my-lg-0">
          <ul class="navbar-nav">
            <li class="nav-item">
              <a class="nav-link" href="#">Link</a>
            </li>
          </ul>
        </div>
      </nav>
    </div>
</template>

<script>
    export default {
        computed: {
          isAuth() {
              return this.$store.getters.tokenIsCurrent();
          },
          hasUserInfo() {
            return this.$store.getters.userInfo;
          },
          displayName() {
            return this.$store.getters.userInfo.displayName;
          },
          storeIsBusy() {
            return this.$store.getters.storeIsBusy;
          }
        },
        methods: {
            logout() {
                this.$store.dispatch("logout")
                    .then(() => {
                        this.$router.push({ name: "login" });
                    });
            }
        }
    }
</script>

<style scoped>
.navbar-nav .nav-item {
    margin-left: 5px;
    margin-right: 5px;
}

.main-logo{
  padding-bottom: 4px;
}

#top-nav {
  max-height: 53px;
  padding-bottom: 10px;
  background-color: #ffffff;
}

#navbar-collapsible {
    z-index: 10;
}

a.top-nav-link {
  color: black !important;
  font-weight: bold;
  text-transform: uppercase;
}

.top-nav-brand-icon {
  margin-top: 6px;
}


</style>
