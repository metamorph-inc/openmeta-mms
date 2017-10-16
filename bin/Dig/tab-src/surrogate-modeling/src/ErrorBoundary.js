import React, { Component } from 'react';

class ErrorBoundary extends Component {
  constructor(props) {
    super(props);
    this.state = { hasError: false, error: null, errorInfo: null };
  }

  componentDidCatch(error, info) {
    // Display fallback UI
    this.setState({ hasError: true, error: error, errorInfo: info });

    console.error("An error occurred:", error);
    console.error(info);
  }

  render() {
    if (this.state.hasError) {
      // You can render any custom fallback UI
      return (
        <div class="panel panel-danger">
          <div class="panel-heading">
            <h3 class="panel-title">Something went wrong.</h3>
          </div>
          <div class="panel-body">
            An error occurred and the surrogate tab was unloaded.  Relaunch the
            visualizer and try again.
          </div>
          <div class="panel-footer">
            <details>
              <pre>
                {this.state.error && this.state.error.toString()}
                <br />
                <br />
                Component stack:
                {this.state.errorInfo.componentStack}
              </pre>
            </details>
          </div>
        </div>
      );
    }
    return this.props.children;
  }
}

export default ErrorBoundary;
