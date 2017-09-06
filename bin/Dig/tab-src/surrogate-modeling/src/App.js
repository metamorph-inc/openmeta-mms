import React, { Component } from 'react';

import { Grid, Row, Col } from 'react-bootstrap';

import { cloneDeep } from 'lodash-es';

import ErrorModal from './ErrorModal';
import DiscreteVariableChooser from './DiscreteVariableChooser';
import SurrogateTable from './SurrogateTable';

import StaticData from './StaticData';
import { DependentVarState } from './Enums';
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

    //TODO: probably a little more efficient to check to see if we've already
    //      marked these as stale and not update if they already are
    const newDependentVarData = cloneDeep(this.state.dependentVarData);
    newDependentVarData[row].forEach(function(col) {
      col[0] = DependentVarState.STALE;
    });

    this.setState({
      independentVarData: newIndependentVarData,
      dependentVarData: newDependentVarData
    });
  }

  handlePredictButtonClick = (row) => {
    const selectedIndependentVarRow = this.state.independentVarData[row];

    const newDependentVarData = cloneDeep(this.state.dependentVarData);
    newDependentVarData[row].forEach(function(col) {
      col[0] = DependentVarState.COMPUTING;
    });

    this.setState({
      dependentVarData: newDependentVarData
    });

    this.props.service.evaluateSurrogateAtPoints([selectedIndependentVarRow], this.state.discreteIndependentVars)
      .then((resultingDependentVars) => {
        const newDependentVarData = cloneDeep(this.state.dependentVarData);
        newDependentVarData[row] = resultingDependentVars[0];

        this.setState({
          dependentVarData: newDependentVarData
        });
      }).catch((error) => {
        const newDependentVarData = cloneDeep(this.state.dependentVarData);
        newDependentVarData[row].forEach(function(col) {
          col[0] = DependentVarState.STALE;
        });

        this.setState({
          currentErrorMessage: error.message,
          dependentVarData: newDependentVarData
        });
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
    newDependentVarData.push(Array(this.state.dependentVarNames.length).fill([DependentVarState.STALE, 0.0, 0.0]));

    this.setState({
      independentVarData: newIndependentVarData,
      dependentVarData: newDependentVarData
    });
  }

  handleSelectedVariableChange = (varIndex, newValue) => {
    const newDiscreteIndependentVars = cloneDeep(this.state.discreteIndependentVars);
    newDiscreteIndependentVars[varIndex].selected = newValue;

    //TODO: probably a little more efficient to check to see if we've already
    //      marked these as stale and not update if they already are
    const newDependentVarData = cloneDeep(this.state.dependentVarData);
    newDependentVarData.forEach(function(row) {
      row.forEach(function(col) {
        col[0] = DependentVarState.STALE;
      });
    });

    this.setState({
      dependentVarData: newDependentVarData,
      discreteIndependentVars: newDiscreteIndependentVars
    });
  }

  handleErrorDialogClose = () => {
    this.setState({
      currentErrorMessage: null
    });
  };

  render() {
    return (
      <div>
        <ErrorModal show={this.state.currentErrorMessage !== null} errorMessage={this.state.currentErrorMessage} onClose={() => this.handleErrorDialogClose()} />
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
                discreteIndependentVars={this.state.discreteIndependentVars}
                service={this.props.service}

                onIndependentVarChange={(col, row, newValue) => this.handleIndependentVarChange(col, row, newValue)}
                onPredictButtonClick={(row) => this.handlePredictButtonClick(row)}
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
