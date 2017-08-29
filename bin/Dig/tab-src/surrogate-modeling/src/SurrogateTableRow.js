import React, { Component } from 'react';
import { FormControl, Button, Glyphicon } from 'react-bootstrap';

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
    const self = this;

    const indepVarCells = self.props.independentVarData.map(function(value, index) {
      return <td key={index}><FormControl type="number" value={value} /></td>;
    });

    const depVarCells = self.props.dependentVarData.map(function(varData, index) {
      return <DependentVarCell key={index} varData={varData} />;
    });

    return (
      <tr>
        <td><Button bsStyle="info" bsSize="small"><Glyphicon glyph="search" /></Button></td>
        {indepVarCells}
        <td><Button bsStyle="primary" bsSize="small"><Glyphicon glyph="question-sign" /><Glyphicon glyph="chevron-right" /></Button></td>
        {depVarCells}
        <td><Button bsStyle="danger" bsSize="small"><Glyphicon glyph="remove" /></Button></td>
      </tr>
    );
  }
};

export default SurrogateTableRow;
