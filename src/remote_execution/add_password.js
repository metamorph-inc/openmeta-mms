var username = 'kevin';
var password = 'asdfff';

var config = require('./config.json');
var passwords = require('./passwords.json');

var bcrypt = require('bcrypt');
var fs = require('fs');

bcrypt.genSalt(config.bcrypt_rounds, function(err, salt) {
    bcrypt.hash(password, salt, function(err, hash) {
        var ws = fs.createWriteStream('passwords_new.json');
        passwords[username] = { salt: salt, hash: hash, rounds: config.bcrypt_rounds };
        ws.end(JSON.stringify(passwords, null, 4), function(err) {
            if (err) {
                console.log(err);
            } else {
                fs.renameSync('passwords_new.json', 'passwords.json')
                console.log('success: ' + JSON.stringify(passwords[username]));
            }
        });
    });
});

