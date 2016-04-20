
Install node js (v0.10.28 x86 known to work)
Install Java
Install Jenkins (1.554.1LTS known to work)
  Recommend command-line:
  java -Xmx1024m -jar jenkins.war --accessLoggerClassName=winstone.accesslog.SimpleAccessLogger --simpleAccessLogger.format=combined --simpleAccessLogger.file="%CD%\jenkins_access.log" --httpListenAddress=127.0.0.1
 Plugins:
   start Jenkins; wait for it to initialize; stop Jenkins
   curl -L -O http://updates.jenkins-ci.org/download/plugins/createjobadvanced/1.8/createjobadvanced.hpi
   curl -L -O http://updates.jenkins-ci.org/download/plugins/jython/1.9/jython.hpi
   curl -L -O http://updates.jenkins-ci.org/download/plugins/reverse-proxy-auth-plugin/1.3.3/reverse-proxy-auth-plugin.hpi
   xcopy /y/d *.hpi %userprofile%\.jenkins\plugins\ 
   Create Job Advanced
     /configure: Check "Grant creator full control"
   Jython 
   Reverse Proxy Auth
     /configureSecurity: HTTP Header by reverse proxy
       Project-based Matrix Authorization Strategy
         Add administrative user to list with full permissions (or you get locked out of Jenkins)
           To administer Jenkins, hijack your session from the JobManager; or add to sessions.json; or use Fiddler and set header X-Forwarded-User
         Anonymous user: check Overall>Read and Job>Create
   Add labels to nodes: SystemC Schematic
Edit config.json
  "target" is the Jenkins URL
  "port" is the port for clients
Run: (npm install requires VS2010. If not present, run npm install on a machine with VS2010 and copy node_modules over)
  npm install
  echo {} > sessions.json
  echo {} > passwords.json
  node app.js
Edit add_password.js and `node add_password.js` to add users or change passwords
  If updated manually, passwords.json must be updated atomically (copy passwords.js copy.json; edit copy.json; move copy.json passwords.json)
Run the JobManager and test


TODO:
Throttle failed auth requests