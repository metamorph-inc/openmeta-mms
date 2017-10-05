import React from 'react';
import ReactDOM from 'react-dom';

import { shallow, render } from 'enzyme';

import App from '../App';
import BackendService from '../BackendService';

it('renders without crashing', () => {
  const div = document.createElement('div');

  const service = new BackendService();
  ReactDOM.render(<App service={service} />, div);
});

it('shallow rendering produces expected components', () => {
  const service = new BackendService();
  const wrapper = shallow(<App service={service} />);

  expect(wrapper.is('div')).toBe(true);
  expect(wrapper.find('ErrorModal').length).toBe(1);
  expect(wrapper.find('SurrogateModelChooser').length).toBe(1);
  expect(wrapper.find('DisplaySettingsPopoverButton').length).toBe(1);
  expect(wrapper.find('DiscreteVariableChooser').length).toBe(1);
  expect(wrapper.find('SurrogateTable').length).toBe(1);

  console.log(wrapper);
});

it('SurrogateTable addRow adds a row', () => {
  const service = new BackendService();
  const wrapper = shallow(<App service={service} />);

  const initialIvarLength = wrapper.state("independentVarData").length;
  const initialDvarLength = wrapper.state("dependentVarData").length;

  expect(initialIvarLength).toBe(initialDvarLength);

  wrapper.find('SurrogateTable').first().simulate('addRow');

  expect(wrapper.state("independentVarData").length).toBe(initialIvarLength + 1);
  expect(wrapper.state("dependentVarData").length).toBe(initialDvarLength + 1);
});
