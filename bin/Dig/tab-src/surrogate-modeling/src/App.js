import React, { Component } from 'react';

import { Grid, Row, Col } from 'react-bootstrap';

import DiscreteVariableChooser from './DiscreteVariableChooser';
import SurrogateTable from './SurrogateTable';

import StaticData from './StaticData';
import 'bootstrap/dist/css/bootstrap.css';
import './App.css';

class App extends Component {
  render() {
    return (
      <div>
        <Grid fluid>
          <Row>
            <Col md={12}>
              <DiscreteVariableChooser discreteVars={StaticData.discreteIndependentVars} />
            </Col>
          </Row>
          <Row>
            <Col md={12}>
              <SurrogateTable data={StaticData} />
            </Col>
          </Row>
        </Grid>
      </div>
    );
  }
}

export default App;
