import React, { Component } from 'react';

import { Grid, Row, Col } from 'react-bootstrap';

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
              <SurrogateTable data={StaticData}></SurrogateTable>
            </Col>
          </Row>
        </Grid>
      </div>
    );
  }
}

export default App;
