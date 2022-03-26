module.exports = {
  root: true,
  env: {
    node: true,
},
  extends: [
    "plugin:vue/recommended",
    "eslint:recommended",
    "plugin:prettier/recommended",
  ],
  parserOptions: {
    parser: "@babel/eslint-parser",
  },
  rules: {
    "no-console": process.env.NODE_ENV === "production" ? "warn" : "off",
    "no-debugger": process.env.NODE_ENV === "production" ? "warn" : "off",
    "vue/multi-word-component-names": ["off"],
    "vue/valid-v-slot": ["error", {
      "allowModifiers": true
    }],
    "vue/require-default-prop": ["off"],
  },
  globals: {
    "_": "writable"
  }
};
