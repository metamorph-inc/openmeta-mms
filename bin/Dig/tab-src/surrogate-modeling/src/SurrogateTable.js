import React, { Component } from 'react';
import { Row, Col, Table, Button, Glyphicon } from 'react-bootstrap';

import SurrogateTableRow from './SurrogateTableRow';

class SurrogateTable extends Component {
  handleIndependentVarChange = (row, column, newValue) => {
    this.props.onIndependentVarChange(row, column, newValue);
  }

  handlePredictButtonClick = (row) => {
    this.props.onPredictButtonClick(row);
  }

  handleDeleteButtonClick = (row) => {
    this.props.onDeleteButtonClick(row);
  }

  handleAddRowButtonClick = () => {
    this.props.onAddRow();
  }

  render() {
    const headerRow = [];

    headerRow.push(<th key="infoHead" />);
    const indepVarHeader = this.props.independentVarNames.map((varName, index) => {
      return <th key={index}>{varName}</th>;
    });
    headerRow.push(<th />);
    const depVarHeader = this.props.dependentVarNames.map((varName, index) => {
      return (<th key={index}>{varName}</th>);
    });
    headerRow.push(<th />);

    const rows = this.props.independentVarData.map((indepVarRow, index) => {
      return <SurrogateTableRow key={index}
        independentVarData={indepVarRow}
        dependentVarData={this.props.dependentVarData[index]}
        onIndependentVarChange={(col, newValue) => this.handleIndependentVarChange(index, col, newValue)}
        onPredictButtonClick={() => this.handlePredictButtonClick(index)}
        onDeleteButtonClick={() => this.handleDeleteButtonClick(index)} />;
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
          <Button bsStyle="success" onClick={()=>this.handleAddRowButtonClick()}><Glyphicon glyph="plus" /> Add row</Button>
        </Col>
      </Row>
      </div>
    );
  }
};

export default SurrogateTable;
