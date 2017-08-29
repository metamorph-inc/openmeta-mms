import React, { Component } from 'react';
import { FormControl, Glyphicon } from 'react-bootstrap';
import TooltipButton from './TooltipButton';

class DependentVarCell extends Component {
  render() {
    return(
      <td>
        {this.props.varData[0]}
        {this.props.varData.length > 1 ? (
          <div>
            <small>&sigma; {this.props.varData[1]}</small>
          </div>
        ): null}
      </td>
    );
  }
};

class SurrogateTableRow extends Component {
  render() {
    const indepVarCells = this.props.independentVarData.map((value, index) => {
      return <td key={index}><FormControl type="number" value={value} /></td>;
    });

    const depVarCells = this.props.dependentVarData.map((varData, index) => {
      return <DependentVarCell key={index} varData={varData} />;
    });

    return (
      <tr>
        <td><TooltipButton bsStyle="info" bsSize="small" tooltipText="Row details"><Glyphicon glyph="search" /></TooltipButton></td>
        {indepVarCells}
        <td><TooltipButton bsStyle="primary" bsSize="small" tooltipText="Predict"><Glyphicon glyph="question-sign" /><Glyphicon glyph="chevron-right" /></TooltipButton></td>
        {depVarCells}
        <td><TooltipButton bsStyle="danger" bsSize="small" tooltipText="Delete row"><Glyphicon glyph="remove" /></TooltipButton></td>
      </tr>
    );
  }
};

export default SurrogateTableRow;
