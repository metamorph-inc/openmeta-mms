import React, { Component } from 'react';
import { FormGroup, FormControl, OverlayTrigger, Tooltip, Overlay } from 'react-bootstrap';

class ValidatingNumberInput extends Component {
  constructor(props) {
    super(props);

    this.state = {
      preValidationString: this.props.value.toString(),
      valid: true,
      errorText: null
    };
  }

  componentDidUpdate(previousProps, previousState) {
    if(previousProps.value !== this.props.value && this.props.value !== Number(this.state.preValidationString)) {
      this.setState({
        preValidationString: this.props.value.toString(),
        valid: true
      });
    }
  }

  handleChange = (ev) => {
    const validationResult = this.props.validationFunction(ev.target.value);

    if(validationResult.valid) {
      this.props.onChange(validationResult.result);
    } else {
      // TODO: feedback on invalid result
    }

    this.setState({
      preValidationString: ev.target.value,
      valid: validationResult.valid,
      errorText: validationResult.message
    });
  }

  render() {
    let validationState = null;
    let tooltip = null;
    if(this.state.valid) {
      tooltip = <Overlay />;
    } else {
      validationState = "error";
      tooltip = <Tooltip id="tooltip">{this.state.errorText}</Tooltip>;
    }



    return (
      <OverlayTrigger placement="bottom" overlay={tooltip} trigger="focus">
        <FormGroup validationState={validationState}>
          <FormControl value={this.state.preValidationString} onChange={this.handleChange} />
          <FormControl.Feedback />
        </FormGroup>
      </OverlayTrigger>
    );
  }

  static RealNumberValidator = (preValidationString) => {
    const parsedNumber = Number(preValidationString);
    if(Number.isNaN(parsedNumber)) {
      return {
        valid: false,
        message: "Value must be a number"
      };
    } else {
      return {
        valid: true,
        result: parsedNumber
      };
    }
  }

  static IntegerRangeValidator = (min, max) => {
    return (preValidationString) => {
      const parsedNumber = Number(preValidationString);
      if(Number.isNaN(parsedNumber) || !Number.isInteger(parsedNumber) || parsedNumber < min || parsedNumber > max) {
        return {
          valid: false,
          message: `Value must be a whole number between ${min} and ${max}`
        };
      } else {
        return {
          valid: true,
          result: parsedNumber
        };
      }
    };
  }
}

export default ValidatingNumberInput;
