import React, { Component } from 'react';
import { Popover, Glyphicon, Button, OverlayTrigger, Form, FormGroup, FormControl, ControlLabel, Checkbox } from 'react-bootstrap';

class DisplaySettingsPopoverButton extends Component {
  handleRoundNumbersChange = (ev) => {
    const newDisplaySettings = {
      roundNumbers: ev.target.checked,
      precision: this.props.displaySettings.precision
    };

    this.props.onDisplaySettingsChange(newDisplaySettings);
  }

  handlePrecisionChange = (ev) => {
    let newValue = Math.round(ev.target.value);

    if(newValue < 1) {
      newValue = 1;
    } else if(newValue > 21) {
      newValue = 21;
    }

    const newDisplaySettings = {
      roundNumbers: this.props.displaySettings.roundNumbers,
      precision: newValue
    };

    this.props.onDisplaySettingsChange(newDisplaySettings);
  }


  render() {
    let precisionFormGroup = null;
    if(this.props.displaySettings.roundNumbers) {
      precisionFormGroup = (<FormGroup>
        <ControlLabel>Precision</ControlLabel>
        {' '}
        <FormControl type="number" value={this.props.displaySettings.precision} onChange={this.handlePrecisionChange} />
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
