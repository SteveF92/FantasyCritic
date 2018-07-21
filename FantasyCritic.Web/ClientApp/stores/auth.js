// auth.js
import axios from "axios";

export default {
    state: {
        jwt: null,
        expiration: null,
        redirect: "",
        userInfo: {}
    },
    getters: {
        hasToken(state) {
            if (state.jwt === null) {
                return false;
            }

            return state.jwt;
        },
        tokenIsCurrent(state) {
            return (now) => {
                if (state.jwt === null || state.expiration === null) {
                    return false;
                }
                var expire = state.expiration;
                var before = expire > now;

                return state.jwt && before;
            }
        },
        token: (state) => state.jwt,
        redirect: (state) => state.redirect,
        userInfo: (state) => state.userInfo
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
                        reject();
                    });
            });
        },
        getUserInfo(context) {
            return new Promise(function (resolve, reject) {
                axios
                    .get("/api/account/CurrentUser")
                    .then((res) => {
                        context.commit("setUserInfo", res.data);
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
        },
        setRefreshToken(state, refreshToken) {
            localStorage.setItem('refresh_token', refreshToken);
        },
        setUserInfo(state, userInfo) {
            state.userInfo = userInfo;
        },
        clearUserAndToken(state) {
            localStorage.removeItem('jwt_token');
            localStorage.removeItem('jwt_expiration');
            state.jwt = null;
            state.expiration = null;
            axios.defaults.headers.common['Authorization'] = "";
            state.userInfo = {};
        },
        setRedirect(state, path) {
            state.redirect = path;
        },
        clearRedirect(state) {
            state.redirect = "";
        }
    }
}
