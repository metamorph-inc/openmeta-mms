import React, { Component } from 'react';
import { Row, Col, Table, Button, Glyphicon } from 'react-bootstrap';

import SurrogateTableRow from './SurrogateTableRow';

class SurrogateTable extends Component {
  render() {
    const headerRow = [];

    headerRow.push(<th key="infoHead" />);
    const indepVarHeader = this.props.data.independentVarNames.map((varName, index) => {
      return <th key={index}>{varName}</th>;
    });
    headerRow.push(<th />);
    const depVarHeader = this.props.data.dependentVarNames.map((varName, index) => {
      return (<th key={index}>{varName}</th>);
    });
    headerRow.push(<th />);

    const rows = this.props.data.independentVarData.map((indepVarRow, index) => {
      return <SurrogateTableRow key={index} independentVarData={indepVarRow} dependentVarData={this.props.data.dependentVarData[index]} />;
    });

    return(
      <div>
      <Row>
        <Col md={12}>
          <Table className="surrogate-table" striped bordered responsive>
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
        </Col>
      </Row>
      <Row>
        <Col md={12}>
          <Button bsStyle="success"><Glyphicon glyph="plus" /> Add row</Button>
        </Col>
      </Row>
      </div>
    );
  }
};

export default SurrogateTable;
