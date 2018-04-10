const fs = require('fs');
var os = require( 'os' );
var networkInterfaces = os.networkInterfaces( );
var argv = require('minimist')(process.argv.slice(2));
//console.log(argv['f']);

var localIP = networkInterfaces['Wi-Fi'][1].address;
var confPath = '../Loadbalancing/GameServer/bin/Photon.LoadBalancing.dll.config';
//console.log(localIP);

if(argv['h'] || argv['help']) {
  help();
} else {
  fs.readFile('backup.txt', 'utf8', function(err, contents) {
    fs.writeFile(confPath, contents, function (err) {
      if (err) throw err;
      fs.readFile(confPath, 'utf8', function(err, contents) {
        contents = contents.replace(/127.0.0.1/g, localIP);
        fs.writeFile(confPath, contents, function (err) {
          if (err) throw err;
          console.log("the file have changed");
        });
      });
    });
  });
}

if(argv['f']) {
  confPath = argv['f'];
} else if (argv['file']) {
  confPath = argv['file'];
}

function help() {
  console.log("there it is : \r\n"+
              "\r\n"+
              "-h, --h, -help, --help              -> display this help message\r\n"+
              "-f, -file [path including the file] -> if u can't find Photon.LoadBalancing.dll.config\r\n");
}

function getAllIndexes(str, substr) {
  var indexes = [];
  for(var i=0; i<str.length;i++) {
    if (str[i] === substr) indexes.push(i);
  }
  return indexes;
}
