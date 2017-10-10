import React from 'react';
import ReactDOM from 'react-dom';
import './index.css';
import App from './App';
import BackendService from './BackendService';
//import registerServiceWorker from './registerServiceWorker';

const service = new BackendService();
ReactDOM.render(<App service={service} />, document.getElementById('root'));
//registerServiceWorker();
