// Simple requestAnimationFrame shim for testing (required by React 16,
// even in testing environments)
// Note that this needs to happen before loading Enzyme, or React doesn't
// realize it's there and complains
global.requestAnimationFrame = function(callback) {
  setTimeout(callback, 0);
};

// Load and configure Enzyme
var Enzyme = require('enzyme');
var Adapter = require('enzyme-adapter-react-16');

Enzyme.configure({ adapter: new Adapter() });
