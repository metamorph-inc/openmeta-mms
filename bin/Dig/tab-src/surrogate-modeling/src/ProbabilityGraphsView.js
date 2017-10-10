import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { Row, Col, Form, FormGroup, FormControl, ControlLabel, Alert } from 'react-bootstrap';

import PlotlyGraph from './PlotlyGraph';
import { Plots } from 'plotly.js/lib/core';

class ProbabilityGraphsView extends Component {
  constructor(props) {
    super(props);

    this.state = {
      graphsAvailable: false,
      xAxisPoints: null,
      yAxisPoints: null,
      yAxisErrors: null,
      currentErrorMessage: null
    };
  }

  static propTypes = {
    predictionsUnavailable: PropTypes.bool.isRequired,
    independentVarNames: PropTypes.arrayOf(PropTypes.string).isRequired,
    dependentVarNames: PropTypes.arrayOf(PropTypes.string).isRequired,
    selectedIndependentVar: PropTypes.string.isRequired,
    selectedXAxisValue: PropTypes.number.isRequired,
    service: PropTypes.object.isRequired
  }

  componentDidMount() {
    window.onresize = () => {
      const plotlyDivs = document.getElementsByClassName("plotlyDiv");

      for(let i = 0; i < plotlyDivs.length; i++) {
        Plots.resize.bind(this)(plotlyDivs.item(i));
      }
    };

    if(!this.props.predictionsUnavailable) {
      this.reloadGraphs();
    }
  }

  componentDidUpdate(previousProps, previousState) {
    if(previousProps.predictionsUnavailable && !this.props.predictionsUnavailable) {
      console.log("New predictions became available; time to reload graphs");
      this.reloadGraphs();
    } else if(previousProps.selectedIndependentVar !== this.props.selectedIndependentVar && !this.props.predictionsUnavailable) {
      console.log("Selected independent var changed; time to reload graphs");
      this.reloadGraphs();
    }
  }

  reloadGraphs() {
    this.setState({
      graphsAvailable: false,
      currentErrorMessage: null
    });
    this.props.service.getSurrogateGraphData(this.props.independentVarData, this.props.discreteIndependentVars, this.props.selectedIndependentVar, this.props.selectedSurrogateModel).then((result) => {
      console.info("Got new graph data", result);
      this.setState({
        graphsAvailable: true,
        xAxisPoints: result.xAxisPoints,
        yAxisPoints: result.yAxisPoints,
        yAxisErrors: result.yAxisErrors
      });
    }).catch((error) => {
      this.setState({
        currentErrorMessage: error.message
      });
    });
  }

  handleSelectedIndependentVarChange = (ev) => {
    this.props.onSelectedIndependentVarChange(ev.target.value);
  }

  handleGraphClick = (graphIndex, xPosition) => {
    this.props.onGraphClick(graphIndex, xPosition);
  }

  render() {
    const xAxisOptions = this.props.independentVarNames.map((name, index) => {
      return <option key={name} value={name}>{name}</option>;
    });

    let plotlyGraphs = null;

    if(this.state.currentErrorMessage !== null) {
      plotlyGraphs = (
        <Alert bsStyle="danger" style={{margin: "1em"}}>
          <strong>Error</strong>: {this.state.currentErrorMessage}
        </Alert>
      );
    } else if(!this.props.predictionsUnavailable && this.state.graphsAvailable) {
      plotlyGraphs = this.state.yAxisPoints.map((points, index) => {
        return (
          <PlotlyGraph
            key={index}
            xAxisName={this.props.selectedIndependentVar}
            yAxisName={this.props.dependentVarNames[index]}
            xAxisPoints={this.state.xAxisPoints}
            yAxisPoints={points}
            yAxisErrors={this.state.yAxisErrors[index]}
            selectedXAxisValue={this.props.selectedXAxisValue}
            onClick={(xPosition) => this.handleGraphClick(index, xPosition)}
            />
        );
      });
    } else if(!this.props.predictionsUnavailable && !this.state.graphsAvailable) {
      plotlyGraphs = (
        <Alert bsStyle="info" style={{margin: "1em"}}>
          Computing...  please wait.
        </Alert>
      );
    }

    return (
      <div className="probability-graphs-view">
        <Row>
          <Col md={12}>
            <Form inline>
              <FormGroup>
                <ControlLabel>X Axis: </ControlLabel>
                {' '}
                <FormControl componentClass="select" value={this.props.selectedIndependentVar} onChange={(ev) => this.handleSelectedIndependentVarChange(ev)}>
                  {xAxisOptions}
                </FormControl>
              </FormGroup>
            </Form>
          </Col>
        </Row>
        <Row>
          {plotlyGraphs}
        </Row>
      </div>
    );
  }
}

export default ProbabilityGraphsView;
