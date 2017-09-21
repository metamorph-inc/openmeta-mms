import React, { Component } from 'react';
import { Popover, Glyphicon, Button, OverlayTrigger, Form, FormGroup, ControlLabel, Checkbox } from 'react-bootstrap';
import ValidatingNumberInput from './ValidatingNumberInput';

class DisplaySettingsPopoverButton extends Component {
  handleRoundNumbersChange = (ev) => {
    const newDisplaySettings = {
      roundNumbers: ev.target.checked,
      precision: this.props.displaySettings.precision
    };

    this.props.onDisplaySettingsChange(newDisplaySettings);
  }

  handlePrecisionChange = (value) => {
    const newDisplaySettings = {
      roundNumbers: this.props.displaySettings.roundNumbers,
      precision: value
    };

    this.props.onDisplaySettingsChange(newDisplaySettings);
  }


  render() {
    let precisionFormGroup = null;
    if(this.props.displaySettings.roundNumbers) {
      precisionFormGroup = (<FormGroup>
        <ControlLabel>Precision</ControlLabel>
        {' '}
        <ValidatingNumberInput
          value={this.props.displaySettings.precision}
          onChange={this.handlePrecisionChange}
          validationFunction={ValidatingNumberInput.IntegerRangeValidator(1, 21)} />
      </FormGroup>);
    }

    const popover = (
      <Popover id="display-settings-popover" title="Display Settings">
        <Form>
          <Checkbox checked={this.props.displaySettings.roundNumbers} onChange={this.handleRoundNumbersChange}>Round numbers</Checkbox>
          {precisionFormGroup}
        </Form>
      </Popover>
    );

    return (
      <OverlayTrigger trigger="click" rootClose placement="bottom" overlay={popover}>
        <Button bsStyle="link" className="pull-right"><Glyphicon glyph="cog" /> Display Settings</Button>
      </OverlayTrigger>
    );
  }
}

export default DisplaySettingsPopoverButton;
