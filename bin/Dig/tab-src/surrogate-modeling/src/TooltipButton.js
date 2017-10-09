import React, { Component } from 'react';
import PropTypes from 'prop-types';

import { Tooltip, Button, OverlayTrigger } from 'react-bootstrap';

class TooltipButton extends Component {
  static propTypes = {
    tooltipText: PropTypes.string.isRequired
  }

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
