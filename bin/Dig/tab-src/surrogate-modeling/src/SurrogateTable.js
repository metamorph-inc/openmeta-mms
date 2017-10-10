import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { Row, Col, Table, Button, Glyphicon } from 'react-bootstrap';

import SurrogateTableRow from './SurrogateTableRow';

class SurrogateTable extends Component {
  static propTypes = {
    displaySettings: PropTypes.object.isRequired,
    independentVarNames: PropTypes.arrayOf(PropTypes.string).isRequired,
    dependentVarNames: PropTypes.arrayOf(PropTypes.string).isRequired,
    independentVarData: PropTypes.array.isRequired,
    dependentVarData: PropTypes.array.isRequired,
    discreteIndependentVars: PropTypes.array.isRequired,
    selectedSurrogateModel: PropTypes.string.isRequired,
    service: PropTypes.object.isRequired,
    allowTraining: PropTypes.bool.isRequired,
    onIndependentVarChange: PropTypes.func.isRequired,
    onPredictButtonClick: PropTypes.func.isRequired,
    onDuplicateButtonClick: PropTypes.func.isRequired,
    onDeleteButtonClick: PropTypes.func.isRequired,
    onAddRow: PropTypes.func.isRequired,
    onTrain: PropTypes.func.isRequired,
  };

  handleIndependentVarChange = (row, column, newValue) => {
    this.props.onIndependentVarChange(row, column, newValue);
  }

  handlePredictButtonClick = (row) => {
    this.props.onPredictButtonClick(row);
  }

  handleDuplicateButtonClick = (row) => {
    this.props.onDuplicateButtonClick(row);
  }

  handleDeleteButtonClick = (row) => {
    this.props.onDeleteButtonClick(row);
  }

  handleAddRowButtonClick = () => {
    this.props.onAddRow();
  }

  handleTrainButtonClick = () => {
    this.props.onTrain();
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
        displaySettings={this.props.displaySettings}
        independentVarNames={this.props.independentVarNames}
        dependentVarNames={this.props.dependentVarNames}
        independentVarData={indepVarRow}
        dependentVarData={this.props.dependentVarData[index]}
        discreteIndependentVars={this.props.discreteIndependentVars}
        selectedSurrogateModel={this.props.selectedSurrogateModel}
        service={this.props.service}
        onIndependentVarChange={(col, newValue) => this.handleIndependentVarChange(index, col, newValue)}
        onPredictButtonClick={() => this.handlePredictButtonClick(index)}
        onDuplicateButtonClick={() => this.handleDuplicateButtonClick(index)}
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
          {' '}
          <Button bsStyle="primary" onClick={()=>this.handleTrainButtonClick()} disabled={!this.props.allowTraining || (this.props.independentVarData.length <= 0)}><Glyphicon glyph="education" /> Train at these points</Button>
        </Col>
      </Row>
      </div>
    );
  }
};

export default SurrogateTable;
