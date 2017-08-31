import React, { Component } from 'react';
import { FormControl, Glyphicon } from 'react-bootstrap';
import TooltipButton from './TooltipButton';
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
        {this.props.varData[1]}
        {this.props.varData.length > 2 ? (
          <div>
            <small>&sigma; {this.props.varData[2]}</small>
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

  handleDeleteButtonClick = () => {
    this.props.onDeleteButtonClick();
  };

  render() {
    const indepVarCells = this.props.independentVarData.map((value, index) => {
      return <td key={index}><FormControl type="number" value={value} onChange={(ev) => this.handleIndependentVarChange(index, ev)}/></td>;
    });

    const depVarCells = this.props.dependentVarData.map((varData, index) => {
      return <DependentVarCell key={index} varData={varData} />;
    });

    return (
      <tr>
        <td><TooltipButton bsStyle="info" bsSize="small" tooltipText="Row details"><Glyphicon glyph="search" /></TooltipButton></td>
        {indepVarCells}
        <td><TooltipButton bsStyle="primary" bsSize="small" tooltipText="Predict" onClick={() => this.handlePredictButtonClick()}><Glyphicon glyph="question-sign" /><Glyphicon glyph="chevron-right" /></TooltipButton></td>
        {depVarCells}
        <td><TooltipButton bsStyle="danger" bsSize="small" tooltipText="Delete row" onClick={() => this.handleDeleteButtonClick()}><Glyphicon glyph="remove" /></TooltipButton></td>
      </tr>
    );
  }
};

export default SurrogateTableRow;
