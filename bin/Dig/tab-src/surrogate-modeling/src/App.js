import React, { Component } from 'react';

import { Grid, Row, Col } from 'react-bootstrap';

import { cloneDeep } from 'lodash-es';

import DiscreteVariableChooser from './DiscreteVariableChooser';
import SurrogateTable from './SurrogateTable';

import StaticData from './StaticData';
import 'bootstrap/dist/css/bootstrap.css';
import './App.css';

class App extends Component {
  constructor(props) {
    super(props);

    this.state = StaticData;
  }

  componentDidMount() {
    this.props.service.getIndependentVarNames().then((varNames) => {
      this.setState({
        independentVarNames: varNames,
        independentVarData: [],
        dependentVarData: []
      });
    });

    this.props.service.getDependentVarNames().then((varNames) => {
      this.setState({
        dependentVarNames: varNames,
        independentVarData: [],
        dependentVarData: []
      });
    });

    this.props.service.getDiscreteIndependentVars().then((vars) => {
      this.setState({
        discreteIndependentVars: vars
      });
    });
  }

  // Use an arrow function here because they always bind 'this' correctly by
  // default... This isn't part of the final ES spec yet, but our babel config
  // does the right thing, so we're going to stick with it for now
  handleIndependentVarChange = (row, column, newValue) => {
    const newIndependentVarData = cloneDeep(this.state.independentVarData);

    newIndependentVarData[row][column] = newValue;

    this.setState({
      independentVarData: newIndependentVarData
    });
  }

  handleDeleteButtonClick = (row) => {
    const newIndependentVarData = cloneDeep(this.state.independentVarData);
    newIndependentVarData.splice(row, 1);

    const newDependentVarData = cloneDeep(this.state.dependentVarData);
    newDependentVarData.splice(row, 1);

    this.setState({
      independentVarData: newIndependentVarData,
      dependentVarData: newDependentVarData
    });
  }

  handleAddRow = () => {
    //TODO: These should probably be set to something other than zero (taken
    //      from dataset or something)
    const newIndependentVarData = cloneDeep(this.state.independentVarData);
    newIndependentVarData.push(Array(this.state.independentVarNames.length).fill(0.0));

    const newDependentVarData = cloneDeep(this.state.dependentVarData);
    newDependentVarData.push(Array(this.state.dependentVarNames.length).fill([0.0, 0.0]));

    this.setState({
      independentVarData: newIndependentVarData,
      dependentVarData: newDependentVarData
    });
  }

  handleSelectedVariableChange = (varIndex, newValue) => {
    const newDiscreteIndependentVars = cloneDeep(this.state.discreteIndependentVars);
    newDiscreteIndependentVars[varIndex].selected = newValue;

    this.setState({
      discreteIndependentVars: newDiscreteIndependentVars
    });
  }

  render() {
    return (
      <div>
        <Grid fluid>
          <Row>
            <Col md={12}>
              <DiscreteVariableChooser
                discreteVars={this.state.discreteIndependentVars}
                onSelectedVariableChange={(varIndex, newValue) => this.handleSelectedVariableChange(varIndex, newValue)} />
            </Col>
          </Row>
          <Row>
            <Col md={12}>
              <SurrogateTable
                independentVarNames={this.state.independentVarNames}
                dependentVarNames={this.state.dependentVarNames}
                independentVarData={this.state.independentVarData}
                dependentVarData={this.state.dependentVarData}

                onIndependentVarChange={(col, row, newValue) => this.handleIndependentVarChange(col, row, newValue)}
                onDeleteButtonClick={(row) => this.handleDeleteButtonClick(row)}
                onAddRow={() => this.handleAddRow()} />
            </Col>
          </Row>
        </Grid>
      </div>
    );
  }
}

export default App;
