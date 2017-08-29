const StaticData = {
  independentVarNames: ["Some Var 1", "Some Var 2", "Some Var 3"],
  dependentVarNames: ["Dep Var 1", "Dep Var 2"],
  independentVarData: [
    [1.0, 2.0, 3.0],
    [4.0, 5.0, 6.0],
    [7.0, 8.0, 9.0]
  ],
  dependentVarData: [
    [[1.0, 2.0], [2.0, 3.0]],
    [[4.0, 5.0], [6.0, 7.0]],
    [[8.0, 9.0], [10.0, 11.0]]
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
