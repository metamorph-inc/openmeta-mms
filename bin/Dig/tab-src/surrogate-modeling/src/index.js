import React from 'react';
import ReactDOM from 'react-dom';
import './index.css';
import ErrorBoundary from './ErrorBoundary';
import App from './App';
import BackendService from './BackendService';
//import registerServiceWorker from './registerServiceWorker';

const service = new BackendService();
ReactDOM.render((
    <ErrorBoundary>
      <App service={service} />
    </ErrorBoundary>
  ), document.getElementById('root'));
//registerServiceWorker();
