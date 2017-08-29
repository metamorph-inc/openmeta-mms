import React, { Component } from 'react';
import { Table } from 'react-bootstrap';

import SurrogateTableRow from './SurrogateTableRow';

class SurrogateTable extends Component {
  render() {
    const self = this;
    const headerRow = [];

    headerRow.push(<th key="infoHead" />);
    const indepVarHeader = this.props.data.independentVarNames.map(function(varName, index) {
      return <th key={index}>{varName}</th>;
    });
    headerRow.push(<th />);
    const depVarHeader = this.props.data.dependentVarNames.map(function(varName, index) {
      return (<th key={index}>{varName}</th>);
    });
    headerRow.push(<th />);

    const rows = this.props.data.independentVarData.map(function(indepVarRow, index) {
      return <SurrogateTableRow key={index} independentVarData={indepVarRow} dependentVarData={self.props.data.dependentVarData[index]} />;
    });

    return(
      <Table className="surrogate-table" striped bordered>
        <thead>
          <tr>
            <td />
            {indepVarHeader}
            <td />
            {depVarHeader}
            <td />
          </tr>
        </thead>
        <tbody>
          {rows}
        </tbody>
      </Table>
    );
  }
};

export default SurrogateTable;
