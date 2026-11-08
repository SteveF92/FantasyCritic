// The components main.ts registers with app.component. unplugin-vue-components declares the
// bootstrap-vue-next and router ones in components.d.ts.
export {};

declare module 'vue' {
  export interface GlobalComponents {
    FontAwesomeIcon: (typeof import('@fortawesome/vue-fontawesome'))['FontAwesomeIcon'];
    FontAwesomeLayers: (typeof import('@fortawesome/vue-fontawesome'))['FontAwesomeLayers'];
    FontAwesomeLayersText: (typeof import('@fortawesome/vue-fontawesome'))['FontAwesomeLayersText'];
    ValidationField: (typeof import('vee-validate'))['Field'];
    ValidationForm: (typeof import('vee-validate'))['Form'];
  }
}
