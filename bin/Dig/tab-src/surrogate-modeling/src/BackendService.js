class BackendService {
  getIndependentVarNames() {
    return Promise.resolve(['My Indep Var 1', 'My Indep Var 2', 'My Indep Var 3', 'My Indep Var 4']);
  }

  getDependentVarNames() {
    return Promise.resolve(['My Dep Var 1', 'My Dep Var 2', 'My Dep Var 3']);
  }

  getDiscreteIndependentVars() {
    return Promise.resolve([
      {
        varName: "CfgId",
        selected: "Some Configuration",
        available: ["My Configuration", "Some Configuration", "Another Configuration"]
      },
      {
        varName: "Stuff",
        selected: "Foo",
        available: ["Foo", "Bar", "Baz"]
      },
      {
        varName: "Something Else",
        selected: "AAA",
        available: ["AAA", "BBB", "CCC", "DDD"]
      },
      {
        varName: "Material",
        selected: "Stone",
        available: ["Steel", "Plastic", "Stone", "Potatoes"]
      }
    ]);
  }
}

export default BackendService;
