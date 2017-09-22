import React, { Component } from 'react';
import { Col } from 'react-bootstrap';
import { plot } from 'plotly.js/lib/core';

class PlotlyGraph extends Component {
  componentDidMount() {
    console.log("Mounted plotly graph component");
    this.buildPlotlyGraph();
  }

  componentDidUpdate() {
    // Callback for component prop updates (should rebuild or update graph)
  }

  componentWillUnmount() {
    console.log("Unmounting plotly graph component");
  }

  buildPlotlyGraph() {
    const xAxisPoints = this.props.xAxisPoints;
    const yAxisPoints = this.props.yAxisPoints;
    const yAxisError = this.props.yAxisErrors;
    const xAxisName = this.props.xAxisName;
    const yAxisName = this.props.yAxisName;
    const selectedXAxisValue = this.props.selectedXAxisValue;

    const upperErrorY = yAxisPoints.map(function(num, index) {
        return num + yAxisError[index];
    });

    const lowerErrorY = yAxisPoints.map(function(num, index) {
        return num - yAxisError[index];
    });

    const upperErrorY2 = yAxisPoints.map(function(num, index) {
        return num + 2 * yAxisError[index];
    });

    const lowerErrorY2 = yAxisPoints.map(function(num, index) {
        return num -  2 * yAxisError[index];
    });

    plot(this.plotlyDiv, [
      {
        x: xAxisPoints,
        y: lowerErrorY2,
        mode: 'lines',
        line: {
          color: 'yellow',
          width: 0
        },
        name: '+/- 2 std dev',
        showlegend: false
      },
      {
        x: xAxisPoints,
        y: upperErrorY2,
        fill: 'tonexty',
        mode: 'lines',
        line: {
          color: 'yellow',
          width: 0
        },
        name: '+/- 2 std dev'
      },
      {
        x: xAxisPoints,
        y: lowerErrorY,
        mode: 'lines',
        line: {
          color: 'orange',
          width: 0
        },
        name: '+/- 1 std dev',
        showlegend: false
      },
      {
        x: xAxisPoints,
        y: upperErrorY,
        fill: 'tonexty',
        mode: 'lines',
        line: {
          color: 'orange',
          width: 0
        },
        name: '+/- 1 std dev',
        //showlegend: false
      },
      {
        x: xAxisPoints,
        y: yAxisPoints,
        name: 'Predicted values',
        mode: 'lines',
        line: {
          color: 'red'
        }
      },
      {
        x: [selectedXAxisValue, selectedXAxisValue],
        y: [Math.min(...lowerErrorY2), Math.max(...upperErrorY2)],
        mode: 'lines',
        line: {
          color: 'black'
        },
        showlegend: false
      }
    ],
      {
        title: yAxisName,
        legend: {
          //xanchor: 'middle',
          //yanchor: 'bottom',
          orientation: 'h'
        },
        margin: {
          l: 50,
          r: 10,
          t: 40,
          b: 40
        },
        xaxis: {
          title: xAxisName,
          //linecolor: '#aaa',
          //gridcolor: '#444',
          //zerolinecolor: '#ccc'
        },
        yaxis: {
          title: yAxisName,
          //linecolor: '#aaa',
          //gridcolor: '#444',
          //zerolinecolor: '#ccc'
        },
        zaxis: {
          title: ''
        },
        font: {
          family: 'Helvetica Neue, Helvetica, Arial, sans-serif',
          size: 9,
          //color: 'white'
        }
        //paper_bgcolor: 'rgba(255, 255, 255, 0)',
        //plot_bgcolor: 'rgba(255, 255, 255, 0)'
      },
      {
        showLink: false,
        displaylogo: false
      });

    this.plotlyDiv.on('plotly_click', (data) => {
      console.log(data);

      this.props.onClick(data.points[0].x);
    });
  }

  render() {
    return (
      <Col md={6}>
        <div ref={(plotlyDiv) => this.plotlyDiv = plotlyDiv} className="plotlyDiv">
        </div>
      </Col>
    );
  }
}

export default PlotlyGraph;
