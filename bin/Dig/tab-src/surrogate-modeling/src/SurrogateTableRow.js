import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { Glyphicon, ButtonGroup } from 'react-bootstrap';
import TooltipButton from './TooltipButton';
import RowDetailsModalButton from './RowDetailsModalButton';
import NumberView from './NumberView';
import ValidatingNumberInput from './ValidatingNumberInput';
import { DependentVarState } from './Enums';

class DependentVarCell extends Component {
  static propTypes = {
    varData: PropTypes.array.isRequired,
    displaySettings: PropTypes.object.isRequired
  }

  render() {
    let cellClass = "";

    if(this.props.varData[0] === DependentVarState.STALE) {
      cellClass = "warning";
    } else if(this.props.varData[0] === DependentVarState.COMPUTING) {
      cellClass = "info";
      return(
        <td className={cellClass}>
          Computing...
        </td>
      );
    }
    return(
      <td className={cellClass}>
        <NumberView displaySettings={this.props.displaySettings}>
          {this.props.varData[1]}
        </NumberView>
        {this.props.varData.length > 2 ? (
          <div>
            <small>&sigma; <NumberView displaySettings={this.props.displaySettings}>{this.props.varData[2]}</NumberView></small>
          </div>
        ): null}
      </td>
    );
  }
};

class SurrogateTableRow extends Component {
  static propTypes = {
    displaySettings: PropTypes.object.isRequired,
    independentVarNames: PropTypes.arrayOf(PropTypes.string).isRequired,
    dependentVarNames: PropTypes.arrayOf(PropTypes.string).isRequired,
    independentVarData: PropTypes.array.isRequired,
    dependentVarData: PropTypes.array.isRequired,
    discreteIndependentVars: PropTypes.array.isRequired,
    selectedSurrogateModel: PropTypes.string.isRequired,
    service: PropTypes.object.isRequired,
    onIndependentVarChange: PropTypes.func.isRequired,
    onPredictButtonClick: PropTypes.func.isRequired,
    onDuplicateButtonClick: PropTypes.func.isRequired,
    onDeleteButtonClick: PropTypes.func.isRequired
  };

  handleIndependentVarChange = (i, ev) => {
    const parsedNumber = Number(ev.target.value);
    if(Number.isNaN(parsedNumber)) {
      console.log("Invalid number");
    } else {
      this.props.onIndependentVarChange(i, parsedNumber);
    }
  };

  handleValidatedIndependentVarChange = (i, value) => {
    this.props.onIndependentVarChange(i, value);
  };

  handlePredictButtonClick = () => {
    this.props.onPredictButtonClick();
  }

  handleDuplicateButtonClick = () => {
    this.props.onDuplicateButtonClick();
  }

  handleDeleteButtonClick = () => {
    this.props.onDeleteButtonClick();
  };

  render() {
    const indepVarCells = this.props.independentVarData.map((value, index) => {
      return (
        <td key={index}>
          <ValidatingNumberInput
            value={value}
            onChange={(value) => this.handleValidatedIndependentVarChange(index, value)}
            onEnterPressed={this.handlePredictButtonClick}
            validationFunction={ValidatingNumberInput.RealNumberValidator}/>
        </td>
      );
    });

    const depVarCells = this.props.dependentVarData.map((varData, index) => {
      return <DependentVarCell key={index} varData={varData} displaySettings={this.props.displaySettings} />;
    });

    return (
      <tr>
        <td>
          <RowDetailsModalButton
            displaySettings={this.props.displaySettings}
            independentVarNames={this.props.independentVarNames}
            dependentVarNames={this.props.dependentVarNames}
            independentVarData={this.props.independentVarData}
            dependentVarData={this.props.dependentVarData}
            discreteIndependentVars={this.props.discreteIndependentVars}
            selectedSurrogateModel={this.props.selectedSurrogateModel}
            service={this.props.service}
            onIndependentVarChange={(i, ev) => this.handleValidatedIndependentVarChange(i, ev)}
            onPredictButtonClick={this.handlePredictButtonClick} />
        </td>
        {indepVarCells}
        <td><TooltipButton bsStyle="primary" bsSize="small" tooltipText="Predict" onClick={() => this.handlePredictButtonClick()}><Glyphicon glyph="question-sign" /><Glyphicon glyph="chevron-right" /></TooltipButton></td>
        {depVarCells}
        <td>
          <ButtonGroup bsSize="small">
            <TooltipButton tooltipText="Duplicate row" onClick={() => this.handleDuplicateButtonClick()}><Glyphicon glyph="duplicate" /></TooltipButton>
            <TooltipButton bsStyle="danger" tooltipText="Delete row" onClick={() => this.handleDeleteButtonClick()}><Glyphicon glyph="remove" /></TooltipButton>
          </ButtonGroup>
        </td>
      </tr>
    );
  }
};

export default SurrogateTableRow;
export { DependentVarCell };
