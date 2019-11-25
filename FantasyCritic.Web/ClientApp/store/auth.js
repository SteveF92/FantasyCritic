// auth.js
import axios from "axios";

export default {
  state: {
    jwt: null,
    expiration: null,
    userInfo: null,
    newAccountCreated: false,
    isBusy: false
  },
  getters: {
    hasToken(state) {
      if (state.jwt === null) {
        return false;
      }

      return state.jwt;
    },
    tokenIsCurrent(state) {
      return () => {
        if (state.jwt === null || state.expiration === null) {
          return false;
        }
        let expire = state.expiration;
        let now = new Date();
        let notExpiredYet = expire > now;
        let valid = state.jwt && notExpiredYet;

        return valid;
      }
    },
    token: (state) => state.jwt,
    userInfo: (state) => state.userInfo,
    newAccountCreated: (state) => state.newAccountCreated,
    storeIsBusy: (state) => state.isBusy,
    isAdmin: (state) => (state.userInfo && state.userInfo.roles.includes("Admin")),
    isBetaTester: (state) => (state.userInfo && state.userInfo.roles.includes("BetaTester"))
  },
  actions: {
    doAuthentication(context, creds) {
      return new Promise(function (resolve, reject) {
        axios.post("/api/account/login", creds)
          .then((res) => {
            context.commit("setTokenInfo", res.data);
            context.commit("setRefreshToken", res.data.refreshToken);
            context.dispatch("getUserInfo")
              .then(response => { resolve(response) });
          })
          .catch(error => {
            reject();
          });
      });
    },
    registerAccount(context, creds) {
      return new Promise(function (resolve, reject) {
        axios.post('/api/account/register', creds)
          .then(() => {
            context.commit("newAccountCreated");
            resolve();
          })
          .catch(error => {
            reject(error);
          });
      });
    },
    changeDisplayName(context, changeInfo) {
      return new Promise(function (resolve, reject) {
        var model = {
          newDisplayName: changeInfo.newDisplayName
        };
        axios
          .post('/api/account/ChangeDisplayName', model)
          .then((res) => {
            context.dispatch("getUserInfo")
              .then(response => { resolve(response) });
          })
          .catch(error => {
            reject();
          });
      });
    },
    changeEmailAddress(context, changeInfo) {
      return new Promise(function (resolve, reject) {
        var model = {
          newEmailAddress: changeInfo.newEmailAddress,
          code: changeInfo.code
        };
        axios
          .post('/api/account/ChangeEmail', model)
          .then((res) => {
            context.commit("setTokenInfo", res.data);
            context.commit("setRefreshToken", res.data.refreshToken);
            context.dispatch("getUserInfo")
              .then(response => { resolve(response) });
          })
          .catch(error => {
            reject();
          });
      });
    },
    refreshToken(context, creds) {
      return new Promise(function (resolve, reject) {
        axios.post("/api/token/refresh", creds)
          .then((res) => {
            context.commit("setTokenInfo", res.data);
            context.commit("setRefreshToken", res.data.refreshToken);
            context.dispatch("getUserInfo")
              .then(response => { resolve(response) });
          })
          .catch(error => {
            context.commit("clearUserAndToken");
            reject();
          });
      });
    },
    getUserInfo(context) {
      context.commit("setBusy", true);
      return new Promise(function (resolve, reject) {
        axios
          .get("/api/account/CurrentUser")
          .then((res) => {
            context.commit("setUserInfo", res.data);
            context.commit("setBusy", false);
            resolve();
          })
          .catch(() => reject());
      });
    },
    logout(context) {
      return new Promise(function (resolve, reject) {
        context.commit("clearUserAndToken");
        resolve();
      });
    }
  },
  mutations: {
    setTokenInfo(state, tokenInfo) {
      localStorage.setItem('jwt_token', tokenInfo.token);
      localStorage.setItem('jwt_expiration', tokenInfo.expiration);
      state.jwt = tokenInfo.token;
      state.expiration = new Date(tokenInfo.expiration);
      axios.defaults.headers.common['Authorization'] = 'Bearer ' + tokenInfo.token;
      state.newAccountCreated = false;
    },
    setRefreshToken(state, refreshToken) {
      localStorage.setItem('refresh_token', refreshToken);
    },
    setUserInfo(state, userInfo) {
      state.userInfo = userInfo;
      state.newAccountCreated = false;
    },
    clearUserAndToken(state) {
      localStorage.removeItem('jwt_token');
      localStorage.removeItem('jwt_expiration');
      state.jwt = null;
      state.expiration = null;
      delete axios.defaults.headers.common["Authorization"];
      state.userInfo = {};
      state.newAccountCreated = false;
    },
    setBusy(state, isBusyFlag) {
      state.isBusy = isBusyFlag;
    },
    newAccountCreated(state) {
      state.newAccountCreated = true;
    }
  }
}
