import React, { Component } from 'react';

import { Tooltip, Button, OverlayTrigger } from 'react-bootstrap';

class TooltipButton extends Component {
  render() {
    const tooltip = <Tooltip id="tooltip">{this.props.tooltipText}</Tooltip>;
    const {tooltipText, ...passThroughProps} = this.props;

    return (
      <OverlayTrigger placement="bottom" overlay={tooltip}>
        <Button {...passThroughProps} />
      </OverlayTrigger>
    );
  }
}

export default TooltipButton;
