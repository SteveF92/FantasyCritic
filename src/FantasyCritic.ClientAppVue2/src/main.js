import { createApp } from 'vue';

import { Chart as ChartJS, Title, Tooltip, Legend, LineElement, LinearScale, CategoryScale, PointElement, Filler } from 'chart.js';
import annotationPlugin from 'chartjs-plugin-annotation';

import App from './App.vue';
import router from './router/index';

import { pinia } from '@/store/pinia';

import { FontAwesomeIcon, FontAwesomeLayers, FontAwesomeLayersText } from './icons';
import { ValidationField, ValidationForm } from './validation';

// Add the necessary CSS
import 'bootstrap/dist/css/bootstrap.css';
import 'bootstrap-vue-next/dist/bootstrap-vue-next.css';

const app = createApp(App);

app.use(pinia);
app.use(router);

//Vee-validate registration
app.component('ValidationField', ValidationField);
app.component('ValidationForm', ValidationForm);

// Registration of global components
app.component('FontAwesomeIcon', FontAwesomeIcon);
app.component('FontAwesomeLayers', FontAwesomeLayers);
app.component('FontAwesomeLayersText', FontAwesomeLayersText);

ChartJS.register(Title, Tooltip, Legend, LineElement, LinearScale, CategoryScale, PointElement, Filler, annotationPlugin);

app.mount('#app');
