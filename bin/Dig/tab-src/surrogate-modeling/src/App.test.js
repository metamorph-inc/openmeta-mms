import React from 'react';
import ReactDOM from 'react-dom';
import App from './App';
import BackendService from './BackendService';

it('renders without crashing', () => {
  const div = document.createElement('div');

  const service = new BackendService();
  ReactDOM.render(<App service={service} />, div);
});
