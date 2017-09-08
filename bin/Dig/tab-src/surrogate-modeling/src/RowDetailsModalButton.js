import React, { Component } from 'react';
import { Modal, Glyphicon, Button, Alert, Row, Col, FormGroup, FormControl } from 'react-bootstrap';
import TooltipButton from './TooltipButton';
import ProbabilityGraphsView from './ProbabilityGraphsView';
import NumberView from './NumberView';
import { DependentVarState } from './Enums';

class RowDetailsModalButton extends Component {
  constructor(props) {
    super(props);

    this.state = {
      showModal: false
    };
  }

  close = () => {
    this.setState({ showModal: false });
  }

  open = () => {
    this.setState({ showModal: true });
  }

  handleIndependentVarChange = (i, ev) => {
    this.props.onIndependentVarChange(i, ev);
  };

  handlePredictButtonClick = () => {
    this.props.onPredictButtonClick();
  }

  render() {
    const indepVarCells = this.props.independentVarData.map((value, index) => {
      return (
        <FormGroup key={index}>
          <h4>{this.props.independentVarNames[index]}</h4>
          <FormControl type="number" value={value} min={1} max={21} onChange={(ev) => this.handleIndependentVarChange(index, ev)} />
        </FormGroup>
      );
    });

    const depVarCells = this.props.dependentVarData.map((varData, index) => {
      return (
        <div key={index}>
          <h4>{this.props.dependentVarNames[index]}</h4>
          <h5>Mean</h5>
          <p><NumberView displaySettings={this.props.displaySettings}>{varData[1]}</NumberView></p>
          {varData.length >= 2 ? (
            <div>
              <h5>Standard Deviation</h5>
              <p><NumberView displaySettings={this.props.displaySettings}>{varData[2]}</NumberView></p>
              <h5>95% Confidence Interval</h5>
              <p><NumberView displaySettings={this.props.displaySettings}>{varData[1] - varData[2]*2}</NumberView> to <NumberView displaySettings={this.props.displaySettings}>{varData[1] + varData[2]*2}</NumberView></p>
            </div>
          ) : null}
        </div>
      );
    });

    let varStateHeader = null;

    if(this.props.dependentVarNames.length === 0) {
      varStateHeader = (
        <Row><Col md={12}>
          <Alert bsStyle="danger">
            No dependent variables found$mdash; cannot compute predictions.
          </Alert>
        </Col></Row>
      );
    } else if(this.props.dependentVarData[0][0] === DependentVarState.STALE) {
      /* eslint-disable jsx-a11y/href-no-hash */
      varStateHeader = (
        <Row><Col md={12}>
          <Alert bsStyle="warning">
            Predictions are out of date&mdash; click <a className="alert-link" href="#" onClick={this.handlePredictButtonClick}>Predict</a> to update.
          </Alert>
        </Col></Row>
      );
      /* eslint-enable jsx-a11y/href-no-hash */
    } else if(this.props.dependentVarData[0][0] === DependentVarState.COMPUTING) {
      varStateHeader = (
        <Row><Col md={12}>
          <Alert bsStyle="info">
            Computing...  please wait.
          </Alert>
        </Col></Row>
      );
    }

    return (
      <div>
        <TooltipButton bsStyle="info" bsSize="small" tooltipText="Row details" onClick={this.open}><Glyphicon glyph="search" /></TooltipButton>

        <Modal show={this.state.showModal} onHide={this.close} dialogClassName="row-details-modal" backdrop={false} animation={false}>
          <Modal.Header closeButton>
            <Modal.Title>Row details</Modal.Title>
          </Modal.Header>
          <Modal.Body>
            {varStateHeader}
            <Row>
              <Col md={2}>
                {indepVarCells}
              </Col>
              <Col md={2}>
                {depVarCells}
              </Col>
              <Col md={8}>
                <ProbabilityGraphsView
                  independentVarNames={this.props.independentVarNames}
                  dependentVarNames={this.props.dependentVarNames}
                  independentVarData={this.props.independentVarData}
                  discreteIndependentVars={this.props.discreteIndependentVars}
                  predictionsUnavailable={this.props.dependentVarNames.length === 0 || this.props.dependentVarData[0][0] === DependentVarState.COMPUTING || this.props.dependentVarData[0][0] === DependentVarState.STALE}
                  service={this.props.service}/>
              </Col>
            </Row>
          </Modal.Body>
          <Modal.Footer>
            <Button bsStyle="primary" onClick={this.handlePredictButtonClick}><Glyphicon glyph="question-sign" /> Predict</Button>
          </Modal.Footer>
        </Modal>
      </div>
    );
  }
}

export default RowDetailsModalButton;
