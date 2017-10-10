import React, { Component } from 'react';
import PropTypes from 'prop-types';

import { FormGroup, ControlLabel, FormControl } from 'react-bootstrap';

class SurrogateModelChooser extends Component {
  static propTypes = {
    availableSurrogateModels: PropTypes.arrayOf(PropTypes.string).isRequired,
    selectedSurrogateModel: PropTypes.string.isRequired,
    onChange: PropTypes.func.isRequired
  }

  handleChange = (ev) => {
    this.props.onChange(ev.target.value);
  }

  render() {
    const options = this.props.availableSurrogateModels.map((optionName) => {
      return <option key={optionName} value={optionName}>{optionName}</option>;
    });

    return (
        <FormGroup>
          <ControlLabel>Surrogate Technique</ControlLabel>{' '}
          <FormControl componentClass="select" value={this.props.selectedSurrogateModel} onChange={(ev) => this.handleChange(ev)}>
            {options}
          </FormControl>
        </FormGroup>
    );
  }
}

export default SurrogateModelChooser;
