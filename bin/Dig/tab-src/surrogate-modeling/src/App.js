import React, { Component } from 'react';

import { Grid, Row, Col, Form } from 'react-bootstrap';

import { cloneDeep, isEqual } from 'lodash';

import ErrorModal from './ErrorModal';
import SurrogateModelChooser from './SurrogateModelChooser';
import DisplaySettingsPopoverButton from './DisplaySettingsPopoverButton';
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
    this.props.service.getDisplaySettingsState().then((newDisplaySettings) => {
      this.setState({
        displaySettings: newDisplaySettings
      });
    });

    this.props.service.getSurrogateModelState().then((newSurrogateModel) => {
      this.setState({
        selectedSurrogateModel: newSurrogateModel
      });
    });

    const ivarNamePromise = this.props.service.getIndependentVarNames().then((varNames) => {
      this.setState({
        independentVarNames: varNames,
        independentVarData: [],
        dependentVarData: []
      });

      return varNames;
    });

    const dvarNamePromise = this.props.service.getDependentVarNames().then((varNames) => {
      this.setState({
        dependentVarNames: varNames,
        independentVarData: [],
        dependentVarData: []
      });

      return varNames;
    });

    const discreteVarPromise = this.props.service.getDiscreteIndependentVars().then((vars) => {
      this.setState({
        discreteIndependentVars: vars
      });

      return vars;
    });

    Promise.all([ivarNamePromise, dvarNamePromise, discreteVarPromise]).then((results) => {
      console.info("All data loaded: ", results);
      const [independentVarNames, dependentVarNames, ] = results;

      //Now that we have all the metadata, reload the independent var state
      this.props.service.getIndependentVarState().then((independentVarData) => {
        if(independentVarData.length > 0 && independentVarData[0].length === independentVarNames.length) {
          const dependentVarsLength = dependentVarNames.length;

          const dependentVarData = Array(independentVarData.length);

          for(let i = 0; i < dependentVarData.length; i++) {
            dependentVarData[i] = Array(dependentVarsLength);
            for(let j = 0; j < dependentVarData[i].length; j++) {
              dependentVarData[i][j] = [DependentVarState.STALE, 0.0, 0.0];
            }
          }

          this.setState({
            independentVarData: independentVarData,
            dependentVarData: dependentVarData
          });
        } else if(independentVarData.length > 0) {
          console.warn(`Number of independent vars (${independentVarNames.length}) changed from saved data (${independentVarData[0].length})`);
        }
      });
    });
  }

  componentDidUpdate(previousProps, previousState) {
    // Update Shiny's stored independent var state when it changes
    // lodash isEqual does a deep comparison
    if(!isEqual(previousState.independentVarData, this.state.independentVarData)) {
      this.props.service.pushIndependentVarState(this.state.independentVarData);
    }

    if(!isEqual(previousState.discreteIndependentVars, this.state.discreteIndependentVars)) {
      this.props.service.pushDiscreteVarState(this.state.discreteIndependentVars);
    }

    if(!isEqual(previousState.displaySettings, this.state.displaySettings)) {
      this.props.service.pushDisplaySettingsState(this.state.displaySettings);
    }

    if(!isEqual(previousState.selectedSurrogateModel, this.state.selectedSurrogateModel)) {
      this.props.service.pushSurrogateModelState(this.state.selectedSurrogateModel);
    }
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

    this.props.service.evaluateSurrogateAtPoints([selectedIndependentVarRow], this.state.discreteIndependentVars, this.state.selectedSurrogateModel)
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

  handleDuplicateButtonClick = (row) => {
    const newIndependentVarData = cloneDeep(this.state.independentVarData);
    newIndependentVarData.splice(row, 0, cloneDeep(this.state.independentVarData[row]));

    const newDependentVarData = cloneDeep(this.state.dependentVarData);
    newDependentVarData.splice(row, 0, cloneDeep(this.state.dependentVarData[row]));

    this.setState({
      independentVarData: newIndependentVarData,
      dependentVarData: newDependentVarData
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

  handleTrain = () => {
    this.setState({
      allowTraining: false
    });

    this.props.service.trainSurrogateAtPoints(this.state.independentVarData, this.state.discreteIndependentVars, this.state.selectedSurrogateModel)
      .then((resultingDependentVars) => {
        const newDependentVarData = cloneDeep(this.state.dependentVarData);
        newDependentVarData.forEach(function(row) {
          row.forEach(function(col) {
            col[0] = DependentVarState.STALE;
          });
        });

        this.setState({
          allowTraining: true,
          dependentVarData: newDependentVarData
        });
      })
      .catch((error) => {
        const newDependentVarData = cloneDeep(this.state.dependentVarData);
        newDependentVarData.forEach(function(row) {
          row.forEach(function(col) {
            col[0] = DependentVarState.STALE;
          });
        });

        this.setState({
          allowTraining: true,
          currentErrorMessage: error.message,
          dependentVarData: newDependentVarData
        });
      });;
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

  handleDisplaySettingsChange = (newDisplaySettings) => {
    this.setState({
      displaySettings: newDisplaySettings
    });
  }

  handleSelectedSurrogateModelChange = (newSelectedSurrogateModel) => {
    //TODO: probably a little more efficient to check to see if we've already
    //      marked these as stale and not update if they already are
    const newDependentVarData = cloneDeep(this.state.dependentVarData);
    newDependentVarData.forEach(function(row) {
      row.forEach(function(col) {
        col[0] = DependentVarState.STALE;
      });
    });

    this.setState({
      selectedSurrogateModel: newSelectedSurrogateModel,
      dependentVarData: newDependentVarData
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
              <Form inline className="pull-right">
                <SurrogateModelChooser
                  selectedSurrogateModel={this.state.selectedSurrogateModel}
                  availableSurrogateModels={this.state.availableSurrogateModels}
                  onChange={this.handleSelectedSurrogateModelChange} />
                <DisplaySettingsPopoverButton
                  displaySettings={this.state.displaySettings}
                  onDisplaySettingsChange={this.handleDisplaySettingsChange} />
              </Form>
            </Col>
          </Row>
          <Row>
            <Col md={12}>
              <DiscreteVariableChooser
                discreteVars={this.state.discreteIndependentVars}
                onSelectedVariableChange={(varIndex, newValue) => this.handleSelectedVariableChange(varIndex, newValue)} />
            </Col>
          </Row>
          <Row>
            <Col md={12}>
              <div>
                <SurrogateTable
                  displaySettings={this.state.displaySettings}
                  independentVarNames={this.state.independentVarNames}
                  dependentVarNames={this.state.dependentVarNames}
                  independentVarData={this.state.independentVarData}
                  dependentVarData={this.state.dependentVarData}
                  discreteIndependentVars={this.state.discreteIndependentVars}
                  selectedSurrogateModel={this.state.selectedSurrogateModel}
                  service={this.props.service}
                  allowTraining={this.state.allowTraining}

                  onIndependentVarChange={(col, row, newValue) => this.handleIndependentVarChange(col, row, newValue)}
                  onPredictButtonClick={(row) => this.handlePredictButtonClick(row)}
                  onDuplicateButtonClick={(row) => this.handleDuplicateButtonClick(row)}
                  onDeleteButtonClick={(row) => this.handleDeleteButtonClick(row)}
                  onAddRow={() => this.handleAddRow()}
                  onTrain={() => this.handleTrain()} />
              </div>
            </Col>
          </Row>
        </Grid>
      </div>
    );
  }
}

export default App;
