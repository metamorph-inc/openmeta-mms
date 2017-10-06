import React from 'react';

import { shallow } from 'enzyme';

import SurrogateTable from '../SurrogateTable';

import { ExampleData } from '../StaticData';

it("renders", () => {
  const wrapper = shallow(<SurrogateTable
    displaySettings={ExampleData.displaySettings}
    independentVarNames={ExampleData.independentVarNames}
    dependentVarNames={ExampleData.dependentVarNames}
    independentVarData={ExampleData.independentVarData}
    dependentVarData={ExampleData.dependentVarData}
    discreteIndependentVars={ExampleData.discreteIndependentVars}
    selectedSurrogateModel={ExampleData.selectedSurrogateModel}
    service={null}
    allowTraining={true}
    />);

  expect(wrapper.is("div")).toBe(true);
  expect(wrapper.find("Table").length).toBe(1);
  expect(wrapper.find("Table[className='surrogate-table']").length).toBe(1);
  expect(wrapper.find("Table[className='surrogate-table'] th").length).toBe(
    ExampleData.independentVarNames.length + ExampleData.dependentVarNames.length
  );
  expect(wrapper.find("SurrogateTableRow").length).toBe(ExampleData.independentVarData.length);
});
