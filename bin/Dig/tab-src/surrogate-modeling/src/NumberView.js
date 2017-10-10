import React, { Component } from 'react';
import PropTypes from 'prop-types';

class NumberView extends Component {
  static propTypes = {
    children: PropTypes.number.isRequired,
    displaySettings: PropTypes.shape({
      roundNumbers: PropTypes.bool,
      precision: PropTypes.number
    }).isRequired
  }

  render() {
    let formattedNumber = null;

    if(typeof(this.props.children) === "number") {
      if(this.props.displaySettings.roundNumbers) {
        formattedNumber = this.props.children.toPrecision(this.props.displaySettings.precision);
      } else {
        formattedNumber = this.props.children.toString();
      }
    } else {
      formattedNumber = "Error: See Console";
    }

    return (
      <span>
        {formattedNumber}
      </span>
    );
  }
}

export default NumberView;
