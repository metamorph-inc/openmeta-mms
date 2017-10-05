// Load and configure Enzyme
var Enzyme = require('enzyme');
var Adapter = require('enzyme-adapter-react-15');

Enzyme.configure({ adapter: new Adapter() });
