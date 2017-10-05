import React from 'react';
import ReactDOM from 'react-dom';

import { shallow, render } from 'enzyme';

import NumberView from '../NumberView';

it('renders unrounded numbers', () => {
  const displaySettings = {
    roundNumbers: false,
    precision: 5
  };

  const wrapper = shallow(<NumberView displaySettings={displaySettings}>{12}</NumberView>);

  expect(wrapper.text()).toBe(Number(12).toString());
});

it('renders rounded numbers', () => {
  const displaySettings = {
    roundNumbers: true,
    precision: 5
  };

  const wrapper = shallow(<NumberView displaySettings={displaySettings}>{13}</NumberView>);

  expect(wrapper.text()).toBe(Number(13).toPrecision(5));
});
