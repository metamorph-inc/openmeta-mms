// cd %USERPROFILE%\Documents\meta-tonka\META\src\CyPhyMasterExe\bin\Release\
// CyPhyMasterExe.exe "MGA=C:\Users\kevin\Documents\tonkalib\designs\LEDMatrix\LEDMatrix.mga" "/@TestBenches/@DummyTestBench" "/@Generated configurations/@ProtoModuleDS/@Configurations (04-16-2014 01:03:03)/@ProtoModuleDS_cfg1" 

// CyPhyMasterExe.exe "MGA=C:\Users\kevin\Documents\tonkalib\designs\LEDMatrix\LEDMatrix.mga" "/@TestBenches/@ChipFit/@ChipFit_TestBench" "/@Modules_PointDesigns/@DevConModuleDesign" 

// avascript:document.cookie="_session_id=abbdb29ddbec47601cd30406dfb99415dbb4e2df2abe88712ebf864beb93671bf4d47f084741a35c8fe90ec832ef9451"

// TODO /job/kevin_pa1uipjcbki5gegd/stop ; abbb ; kevin  ::2014-06-09T22:32:45.050Z

var config = require('./config.json');
var passwords = require('./passwords.json');

var Cookies = require("cookies");
var url = require('url');
var fs = require('fs');
var superagent = require('superagent');
// require('superagent-proxy')(superagent);
var http = require('http');
var httpProxy = require('http-proxy');

var crypto = require('crypto');
var bcrypt = require('bcrypt');
var querystring = require('querystring');
var express = require('express');
var cookieParser = require('cookie-parser');
var sessions = require('./sessions.json');
var filename_map = {}; // TODO serialize/deserialize

log_error = function(err_string) {
  console.log("ERROR: " + err_string + "  ::" + new Date().toISOString());
};
log_info = function(info_string) {
  console.log("" + info_string + "  ::" + new Date().toISOString());
};

setInterval(function() {
  Object.keys(sessions).forEach(function(key) {
    var val = sessions[key];
    // TODO expire
    // delete sessions[key]
  });
  var ws = fs.createWriteStream('sessions.json');
  ws.on('open', function() {
    ws.end(JSON.stringify(sessions, null, 4));
  });
}, 1000 * 20);

fs.watch('passwords.json', {}, function() {
  var rs = fs.createReadStream('passwords.json');
  var passwords_data = '';
  rs.on('data', function(chunk) {
    passwords_data = passwords_data + chunk;
  });
  rs.on('end', function() {
    passwords = JSON.parse(passwords_data);
    log_info('Passwords updated');
  });
});

var app = express();

app.use(cookieParser());
app.use(function (req, res, next) {
    var session = req.cookies['_session_id'];
    if (req.path.substr(0, '/auth'.length) !== '/auth'
        && req.path.substr(0, '/testbench/get_file'.length) !== '/testbench/get_file'
        && req.path.substr(0, '/testbench/put_file'.length) !== '/testbench/put_file'
        && !sessions[session]) {
      res.send(403, "Invalid session id.");
      return;
    }
    if (!session) {
      crypto.randomBytes(48, function(ex, buf) {
        session = buf.toString('hex');
        res.cookie('_session_id', session, { path: '/' });
        req._session_id = session;
        next();
      });
    } else {
      req._session_id = session;
      next();
    }
});
app.get('/auth/', function(req, res){
      res.writeHead(200, {
        'Content-Type': 'text/plain'
      });
      res.end('Hello');
});
app.post('/auth/do_login', function (req, res) {
    if (req.method == 'POST') {
        var body = '';
        req.on('data', function (data) {
            body += data;
            // ~1MB
            if (body.length > 1e6) { 
                // flood attack or faulty client, nuke request
                req.connection.destroy();
            }
        });
        req.on('end', function () {
            var POST = querystring.parse(body);
            if (!POST.password) {
                res.send(403, 'No password provided');
                return;
            }
            if (!POST.username) {
                res.send(403, 'No username provided');
                return;
            }
            bcrypt.compare(POST.password, (passwords[POST.username] || {}).hash, function(err, hash_res) {
                if (!hash_res) {
                    res.send(403, 'Wrong password'); // TODO: copy VF's braindead response
                    return;
                }
                sessions[req._session_id] = {};
                sessions[req._session_id].username = POST.username;
                log_info(POST.username + " logged in. session " + req._session_id);
                res.redirect('/auth/done');
            });
        });
    }
});

app.get('/auth/done', function(req, res) {
    res.send('Done logging in as ' + sessions[req._session_id].username);
});
app.get('/rest/user/get_user_profile', function(req, res){
  res.json({username: sessions[req._session_id].username,
        //interests { get; set; }
        //profileImage { get; set; }
        //skypeName { get; set; }
        //mission { get; set; }
        //expertise { get; set; }
        //userSince { get; set; }
        fullName: sessions[req._session_id].username,
        trustInfo: { },
        projects: []
        });
});
app.get('/testbench/info.json', function(req, res){
    res.json({
        status: 'UP',
        nodes: [],
                 /*  {
                public string status { get; set; }
                public int available { get; set; }
                public int busy { get; set; }
                public string name { get; set; }
                public string description { get; set; }
                    } */
        totalBusy: 0,
        totalAvailable: 1,
        highPriorityRemaining:0
        });
});
app.get('/testbench/client_upload_url', function(req, res){
    var parsed_request = url.parse(req.url, true);
    crypto.randomBytes(48, function(ex, buf) {
        var filename = buf.toString('hex');
        filename_map[parsed_request.query.filename] = filename;
        res.json({
            url: 'http://' + req.headers['host'] + "/testbench/put_file?filename=" + filename,
            });
    });
});
app.get('/testbench/client_download_url', function(req, res){
    var parsed_request = url.parse(req.url, true);
    crypto.randomBytes(48, function(ex, buf) {
        res.json({
            url: 'http://' + req.headers['host'] + "/testbench/get_file?filename=" + filename_map[parsed_request.query.filename] + "_result",
            });
    });
});
app.put('/testbench/put_file', function(req, res){
  // if (req.headers['Expect'] === '100-continue') { TODO: this is handled automatically?
  var parsed_request = url.parse(req.url, true);
  var filename = parsed_request.query.filename;
  var ws = fs.createWriteStream(config.files_dir + filename);
  ws.on('error', function(err) {
    log_error("put_file " + err);
    res.send(404, 'error');
  });
  ws.on('open', function() {
      req.on('data', function(chunk){ 
          ws.write(chunk);
          req.text += chunk; });
      req.on('end', function() {
        ws.end();
        res.send('done');
      });
    });
});
app.get('/testbench/get_file', function(req, res){
  var parsed_request = url.parse(req.url, true);
  var filename = parsed_request.query.filename;
  var ws = fs.createReadStream(config.files_dir + filename);
  ws.pipe(res);
  ws.on('error', function(err) {
    log_error("get_file " + err);
    res.sendStatus(404);
  });
});
app.post('/testbench/createItem', function(req, res){
//POST http://mmhp:5050/testbench/createItem?name=kevin_x5bh53ohnat5cmzo HTTP/1.1
//Content-Type: application/x-www-form-urlencoded
    if (req.method == 'POST') {
        var body = '';
        req.on('data', function (data) {
            body += data;
            // ~1MB
            if (body.length > 1e6) { 
                // flood attack or faulty client, nuke request
                req.connection.destroy();
            }
        });
        req.on('end', function () {
            var parsed_request = url.parse(req.url, true);
            var POST = querystring.parse(body);
            var config_xml = POST.data;
            // curl -X POST "http://user:password@hudson.server.org/createItem?name=newjobname" --data-binary "@newconfig.xml" -H "Content-Type: text/xml"
            superagent
               .post(config.target + '/createItem?name=' + parsed_request.query.name)
               // .proxy('http://localhost:9999') // use Fiddler on port 9999 to debug Jenkins API issues
               .set('Content-Type', 'text/xml')
               .set('X-Forwarded-User', sessions[req._session_id].username)
               .send(config_xml)
               .end(function(err, get_res){
                 if (err) {
                   log_error(err);
                   res.sendStatus(500);
                 } else if (get_res.ok) {
                   res.send(200, 'ok');
                 } else {
                   log_error('/createItem: ' + get_res.text);
                   res.sendStatus(500);
                 }
               });
        });
    }
});
app.post(/^\/testbench\/job\/([^/]+)\/build/, function(req, res) {
    var jobname = req.params[0];
    var parsed_request = url.parse(req.url, true);
    var jenkins_req = superagent
       .post(config.target + '/job/' + jobname + '/build?delay=0sec')
       // .proxy('http://localhost:9999')
       //.set('Content-Type', 'text/xml')
       .set('X-Forwarded-User', sessions[req._session_id].username);
    var filename = filename_map[parsed_request.query.filename];
    filename_map[parsed_request.query.filename + "_result"] = filename + "_result";
    var this_server = 'http://' + req.headers['host'];
    var parameters = { "resultsGetUrl": this_server + '/testbench/get_file?filename=' + filename + "_result",
                       "resultsPutUrl": this_server + '/testbench/put_file?filename=' + filename + "_result",
                       "sourceGetUrl": this_server + '/testbench/get_file?filename=' + filename };
    var parameters_encoded = { parameter: []};
    Object.keys(parameters).forEach(function(key) {
        jenkins_req.send('name=' + key)
            .send('value=' + parameters[key]);
        parameters_encoded.parameter.push({name: key, value: parameters[key]});
    });
    jenkins_req.send('json=' + encodeURIComponent(JSON.stringify(parameters_encoded)))
        .send('Submit=Build')
        .end(function(err, jenkins_res){
            if (err) {
              log_error(err);
              res.sendStatus(500);
            } else if (jenkins_res.ok) {
               res.sendStatus(200);
             } else {
               log_error('.../build: ' + jenkins_res.text);
               res.sendStatus(500);
             }
       });

});
var app_http = app.listen(0, "127.0.0.1");

// TODO test against  app._router.stack: regexp && route 
routes = { '/auth/do_login': 1,
'/auth/': 1,
'/auth/done': 1,
'/rest/user/get_user_profile': 1,
'/testbench/info.json': 1,
'/testbench/put_file': 1,
'/testbench/get_file': 1,
'/testbench/createItem': 1,
'/testbench/job': 1,
'/testbench/client_upload_url': 1,
'/testbench/client_download_url': 1
 };


var proxy = httpProxy.createProxyServer({});



var server = require('http').createServer(function(req, res) {
  var cookies = new Cookies(req, res);
  _session_id = cookies.get('_session_id');

  var target = config.target;
  var parsed_request = url.parse(req.url);
  // console.log(parsed_request.pathname);
  if (routes[parsed_request.pathname] || /^\/testbench\/job\/[^/]+\/build/.test(parsed_request.pathname)) {
    target = "http://127.0.0.1:" + app_http.address().port;
  } else {
    if (req.url.indexOf("/testbench") === 0) {
     req.url = req.url.substr("/testbench".length);
    }
    if (!sessions[_session_id]) {
      res.writeHead(403);
      res.end("Invalid session id");
      return;
    }
    req.headers['X-Forwarded-User'] = sessions[_session_id].username;
  }

  log_info(req.url + " ; " + _session_id + " ; " + (sessions[_session_id] || {username: 'not logged in'}).username);
  // console.log(req)
  proxy.web(req, res, { target: target }, function(err) {
    if (err) {
        log_error(err);
    }
  });
});

fs.stat(config.files_dir, (err, stats) => {
    if (err) {
        log_error('Error stating config.files_dir: ' + err)
        process.exit(2);
    } else if (!stats.isDirectory()) {
        log_error('config.files_dir is not a directory')
        process.exit(2);
    } else {
        server.listen(config.port);
        log_info("listening on port " + config.port);
    }
});
