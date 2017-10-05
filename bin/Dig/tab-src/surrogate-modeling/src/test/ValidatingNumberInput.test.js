import React from 'react';
import ReactDOM from 'react-dom';

import { shallow, render } from 'enzyme';

import ValidatingNumberInput from '../ValidatingNumberInput';

it('renders without crashing', () => {
  const wrapper = shallow(<ValidatingNumberInput value={10}
    validationFunction={ValidatingNumberInput.RealNumberValidator}
    onChange={null} />);

  expect(wrapper.find("FormControl").length).toBe(1);
  expect(wrapper.find("FormControl").first().props().value).toBe(Number(10).toString());
});

it('passes valid input', () => {
  const handleChangeMock = jest.fn();

  const wrapper = shallow(<ValidatingNumberInput value={10}
    validationFunction={ValidatingNumberInput.RealNumberValidator}
    onChange={handleChangeMock} />);

  expect(wrapper.find("FormControl").length).toBe(1);
  expect(wrapper.find("FormControl").first().props().value).toBe(Number(10).toString());

  const changeEventArgs = {
    target: {
      value: "11.2"
    }
  };
  wrapper.find("FormControl").first().simulate("change", changeEventArgs);

  expect(wrapper.find("FormControl").first().props().value).toBe("11.2");
  expect(handleChangeMock).toHaveBeenCalledWith(11.2);
  expect(wrapper.find("FormGroup").first().props().validationState).toBeNull();
});

it('doesn\'t pass valid input', () => {
  const handleChangeMock = jest.fn();

  const wrapper = shallow(<ValidatingNumberInput value={10}
    validationFunction={ValidatingNumberInput.RealNumberValidator}
    onChange={handleChangeMock} />);

  expect(wrapper.find("FormControl").length).toBe(1);
  expect(wrapper.find("FormControl").first().props().value).toBe(Number(10).toString());

  const changeEventArgs = {
    target: {
      value: "1.2a"
    }
  };
  wrapper.find("FormControl").first().simulate("change", changeEventArgs);

  expect(wrapper.find("FormControl").first().props().value).toBe("1.2a");
  expect(handleChangeMock).not.toHaveBeenCalled();
  expect(wrapper.find("FormGroup").first().props().validationState).toBe("error");
});

it('RealNumberValidator accepts real numbers', () => {
  const validator = ValidatingNumberInput.RealNumberValidator;
  expect(validator("1").valid).toBe(true);
  expect(validator("1").result).toBe(1);
  expect(validator("0.1").valid).toBe(true);
  expect(validator("0.1").result).toBeCloseTo(0.1, 5);
  expect(validator("-0.1").valid).toBe(true);
  expect(validator("-0.1").result).toBeCloseTo(-0.1, 5);
  expect(validator("3.254e10").valid).toBe(true);
  expect(validator("3.254e10").result).toBeCloseTo(3.254e10, 5);
  expect(validator("-3.254e10").valid).toBe(true);
  expect(validator("-3.254e10").result).toBeCloseTo(-3.254e10, 5);
});

it('RealNumberValidator rejects values that aren\'t real numbers', () => {
  const validator = ValidatingNumberInput.RealNumberValidator;

  expect(validator("a").valid).toBe(false);
  expect(validator("a")).toHaveProperty("message");
  expect(validator("1.2.").valid).toBe(false);
  expect(validator("1.2.")).toHaveProperty("message");
  expect(validator("1-2").valid).toBe(false);
  expect(validator("1-2")).toHaveProperty("message");
});

it('IntegerRangeValidator accepts integers in range', () => {
  const validator = ValidatingNumberInput.IntegerRangeValidator(-3, 5);

  expect(validator("1").valid).toBe(true);
  expect(validator("1").result).toBe(1);
  expect(validator("-3").valid).toBe(true);
  expect(validator("-3").result).toBe(-3);
  expect(validator("5").valid).toBe(true);
  expect(validator("5").result).toBe(5);
});

it('IntegerRangeValidator rejects values that aren\'t integers in range', () => {
  const validator = ValidatingNumberInput.IntegerRangeValidator(-3, 5);

  expect(validator("a").valid).toBe(false);
  expect(validator("a")).toHaveProperty("message");
  expect(validator("-4").valid).toBe(false);
  expect(validator("-4")).toHaveProperty("message");
  expect(validator("6").valid).toBe(false);
  expect(validator("6")).toHaveProperty("message");
  expect(validator("2.3").valid).toBe(false);
  expect(validator("2.3")).toHaveProperty("message");
});
