import React, { Component } from 'react';
import PropTypes from 'prop-types';

import { Well, Form, FormGroup, ControlLabel, FormControl } from 'react-bootstrap';

class DiscreteVariableFormGroup extends Component {
  static propTypes = {
    variable: PropTypes.object.isRequired,
    onChange: PropTypes.func.isRequired
  }

  handleChange = (ev) => {
    this.props.onChange(ev.target.value);
  }

  render() {
    const options = this.props.variable.available.map((optionName) => {
      return <option key={optionName} value={optionName}>{optionName}</option>;
    });

    return (
      <FormGroup>
        <ControlLabel>{this.props.variable.varName}</ControlLabel>
        {' '}
        <FormControl componentClass="select" value={this.props.variable.selected} onChange={(ev) => this.handleChange(ev)}>
          {options}
        </FormControl>
      </FormGroup>
    );
  }
}

class DiscreteVariableChooser extends Component {
  static propTypes = {
    discreteVars: PropTypes.array.isRequired,
    onSelectedVariableChange: PropTypes.func.isRequired
  }

  handleChange = (varIndex, newValue) => {
    this.props.onSelectedVariableChange(varIndex, newValue);
  }

  render() {
    const discreteVariableFormGroups = this.props.discreteVars.map((discreteVar, index) => {
      return <DiscreteVariableFormGroup
        key={discreteVar.varName}
        variable={discreteVar}
        onChange={(newValue) => this.handleChange(index, newValue)} />;
    });

    return (
      <Well className="discrete-var-chooser">
        <h4>Discrete variables:</h4>
        <Form inline>
          {discreteVariableFormGroups}
        </Form>
      </Well>
    );
  }
}

export default DiscreteVariableChooser;
