import React, { Component } from 'react';

import { Well, Form, FormGroup, ControlLabel, FormControl } from 'react-bootstrap';

class SurrogateModelChooser extends Component {
  handleChange = (ev) => {
    this.props.onChange(ev.target.value);
  }

  render() {
    const options = this.props.availableSurrogateModels.map((optionName) => {
      return <option key={optionName} value={optionName}>{optionName}</option>;
    });

    return (
        <FormControl componentClass="select" value={this.props.selectedSurrogateModel} onChange={(ev) => this.handleChange(ev)}>
          {options}
        </FormControl>
    );
  }
}

export default SurrogateModelChooser;
