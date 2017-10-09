import { DependentVarState } from './Enums';

const StaticData = {
  independentVarNames: [],
  dependentVarNames: [],
  independentVarData: [],
  dependentVarData: [],
  discreteIndependentVars: [],
  currentErrorMessage: null,
  displaySettings: {
    roundNumbers: false,
    precision: 5
  },
  selectedSurrogateModel: "Kriging Surrogate",
  availableSurrogateModels: ["Kriging Surrogate", "Random Forest"],
  allowTraining: true
};

const ExampleData = {
  independentVarNames: ["Some Var 1", "Some Var 2", "Some Var 3"],
  dependentVarNames: ["Dep Var 1", "Dep Var 2"],
  independentVarData: [
    [1.342, 2.622, 3.3429],
    [4.4921, 5.42112, 6.32],
    [7.0, 8.323, 9.9084]
  ],
  dependentVarData: [
    [[DependentVarState.COMPUTED, 1.3290, 2.123], [DependentVarState.COMPUTED, 2.382, 3.348]],
    [[DependentVarState.COMPUTING, 4.324, 5.1], [DependentVarState.COMPUTING, 6.953]],
    [[DependentVarState.STALE, 8.3248, 9.39048204], [DependentVarState.STALE, 10.49, 11.4932]]
  ],
  discreteIndependentVars: [
    {
      varName: "CfgId",
      selected: "Config2",
      available: ["Config1", "Config2", "Config3"]
    },
    {
      varName: "Color",
      selected: "Orange",
      available: ["Red", "Blue", "Green", "Orange"]
    },
    {
      varName: "Material",
      selected: "Steel",
      available: ["Steel"]
    }
  ]
};

export default StaticData;
export { ExampleData };
