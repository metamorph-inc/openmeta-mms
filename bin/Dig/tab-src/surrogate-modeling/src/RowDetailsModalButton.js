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
      showModal: false,
      selectedIndependentVar: (this.props.independentVarNames.length > 0 ? this.props.independentVarNames[0] : "")
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
  }

  handlePredictButtonClick = () => {
    this.props.onPredictButtonClick();
  }

  handleSelectedIndependentVarChange = (newValue) => {
    this.setState({
      selectedIndependentVar: newValue
    });
  }

  // Clicking the graph sets the selected independent var to the chosen value,
  // and predicts at that point
  handleGraphClick = (graphIndex, xPosition) => {
    this.handleIndependentVarChange(this.props.independentVarNames.indexOf(this.state.selectedIndependentVar), {target: {value: xPosition}});
    this.handlePredictButtonClick();
  }

  render() {
    const indepVarCells = this.props.independentVarData.map((value, index) => {
      let className = "";

      if(this.props.independentVarNames[index] === this.state.selectedIndependentVar) {
        className = "selected-var";
      }

      return (
        <FormGroup key={index} className={className}>
          <h4>{this.props.independentVarNames[index]}</h4>
          <FormControl type="number" value={value} min={1} max={21} onChange={(ev) => this.handleIndependentVarChange(index, ev)} />
        </FormGroup>
      );
    });

    const depVarCells = this.props.dependentVarData.map((varData, index) => {
      let className = "";
      if(varData[0] === DependentVarState.STALE) {
        className = "text-warning";
      } else if(varData[0] === DependentVarState.COMPUTING) {
        className = "text-info";
      }

      return (
        <div key={index} className={className}>
          <h4>{this.props.dependentVarNames[index]}</h4>
          <dl>
          <dt>Mean</dt>
          <dd><NumberView displaySettings={this.props.displaySettings}>{varData[1]}</NumberView></dd>
          {varData.length >= 2 ? (
            <div>
              <dt>Standard Deviation</dt>
              <dd><NumberView displaySettings={this.props.displaySettings}>{varData[2]}</NumberView></dd>
              <dt>95% Confidence Interval</dt>
              <dd><NumberView displaySettings={this.props.displaySettings}>{varData[1] - varData[2]*2}</NumberView> to <NumberView displaySettings={this.props.displaySettings}>{varData[1] + varData[2]*2}</NumberView></dd>
            </div>
          ) : null}
          </dl>
        </div>
      );
    });

    let varStateHeader = null;

    if(this.props.dependentVarNames.length === 0) {
      varStateHeader = (
        <Row><Col md={12}>
          <Alert bsStyle="danger">
            No dependent variables found&mdash; cannot compute predictions.
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
                  selectedIndependentVar={this.state.selectedIndependentVar}
                  independentVarNames={this.props.independentVarNames}
                  dependentVarNames={this.props.dependentVarNames}
                  independentVarData={this.props.independentVarData}
                  discreteIndependentVars={this.props.discreteIndependentVars}
                  predictionsUnavailable={this.props.dependentVarNames.length === 0 || this.props.dependentVarData[0][0] === DependentVarState.COMPUTING || this.props.dependentVarData[0][0] === DependentVarState.STALE}
                  service={this.props.service}
                  onSelectedIndependentVarChange={this.handleSelectedIndependentVarChange}
                  onGraphClick={this.handleGraphClick} />
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
