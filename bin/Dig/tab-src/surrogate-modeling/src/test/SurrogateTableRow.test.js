import React from 'react';

import { shallow } from 'enzyme';

import SurrogateTableRow, { DependentVarCell } from '../SurrogateTableRow';
import { DependentVarState } from '../Enums';

import StaticData, { ExampleData } from '../StaticData';

const displaySettings = {
  round: false,
  precision: 5
};

it("DependentVarCell renders computed state with standard deviation", () => {
  const varData = [
    DependentVarState.COMPUTED,
    2.3,
    0.2
  ];

  const wrapper = shallow(<DependentVarCell varData={varData} displaySettings={displaySettings} />);

  expect(wrapper.is("td")).toBe(true);
  expect(wrapper.prop("className")).toBe("");
  expect(wrapper.find("NumberView").length).toBe(2);
});

it("DependentVarCell renders computed state without standard deviation", () => {
  const varData = [
    DependentVarState.COMPUTED,
    4.0
  ];

  const wrapper = shallow(<DependentVarCell varData={varData} displaySettings={displaySettings} />);

  expect(wrapper.is("td")).toBe(true);
  expect(wrapper.prop("className")).toBe("");
  expect(wrapper.find("NumberView").length).toBe(1);
});

it("DependentVarCell renders stale state with standard deviation", () => {
  const varData = [
    DependentVarState.STALE,
    2.3,
    0.2
  ];

  const wrapper = shallow(<DependentVarCell varData={varData} displaySettings={displaySettings} />);

  expect(wrapper.is("td")).toBe(true);
  expect(wrapper.prop("className")).toBe("warning");
  expect(wrapper.find("NumberView").length).toBe(2);
});

it("DependentVarCell renders stale state without standard deviation", () => {
  const varData = [
    DependentVarState.STALE,
    4.0
  ];

  const wrapper = shallow(<DependentVarCell varData={varData} displaySettings={displaySettings} />);

  expect(wrapper.is("td")).toBe(true);
  expect(wrapper.prop("className")).toBe("warning");
  expect(wrapper.find("NumberView").length).toBe(1);
});

it("DependentVarCell renders computing state and doesn't display numbers", () => {
  const varData = [
    DependentVarState.COMPUTING,
    2.3,
    0.2
  ];

  const wrapper = shallow(<DependentVarCell varData={varData} displaySettings={displaySettings} />);

  expect(wrapper.is("td")).toBe(true);
  expect(wrapper.prop("className")).toBe("info");
  expect(wrapper.find("NumberView").length).toBe(0);
  expect(wrapper.text()).toBe("Computing...");
});

it("renders", () => {
  const wrapper = shallow(<SurrogateTableRow
    displaySettings={StaticData.displaySettings}
    independentVarNames={ExampleData.independentVarNames}
    dependentVarNames={ExampleData.dependentVarNames}
    independentVarData={ExampleData.independentVarData[1]}
    dependentVarData={ExampleData.dependentVarData[1]}
    discreteIndependentVars={ExampleData.discreteIndependentVars}
    selectedSurrogateModel={StaticData.selectedSurrogateModel}
    service={{}}

    onIndependentVarChange={(col, newValue) => null}
    onPredictButtonClick={() => null}
    onDuplicateButtonClick={() => null}
    onDeleteButtonClick={() => null}
    />);

  expect(wrapper.is("tr")).toBe(true);
  expect(wrapper.find("ValidatingNumberInput").length).toBe(3); //3 independent vars
  expect(wrapper.find("DependentVarCell").length).toBe(2);
});
