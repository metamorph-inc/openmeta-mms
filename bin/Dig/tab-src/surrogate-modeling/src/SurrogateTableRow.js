import React, { Component } from 'react';
import { FormControl, Glyphicon, ButtonGroup } from 'react-bootstrap';
import TooltipButton from './TooltipButton';
import RowDetailsModalButton from './RowDetailsModalButton';
import NumberView from './NumberView';
import { DependentVarState } from './Enums';

class DependentVarCell extends Component {
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
  handleIndependentVarChange = (i, ev) => {
    const parsedNumber = Number(ev.target.value);
    if(Number.isNaN(parsedNumber)) {
      console.log("Invalid number");
    } else {
      this.props.onIndependentVarChange(i, parsedNumber);
    }
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
      return <td key={index}><FormControl type="number" value={value} onChange={(ev) => this.handleIndependentVarChange(index, ev)}/></td>;
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
            service={this.props.service}
            onIndependentVarChange={(i, ev) => this.handleIndependentVarChange(i, ev)}
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
