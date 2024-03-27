# Subspace.Agent
Introduction: Briefly introduce Subspace.Agent as a utility tool for Subspace Nodes.

Main Functionality: Describe its primary function of providing assurance for Subspace Nodes by facilitating seamless node switching within a farm using node detection.

Effectiveness: Highlight its significant effectiveness in combating node desynchronization and network disruptions.

User Benefits: Explain how users can benefit by constructing multiple high-quality nodes and selecting the most optimal one for broadcasting rewards.

Using : 

Step 1 :
For Ubuntu :
Use the following command:
apt install unzip

For Windows:
Windows does not require the installation of any software.

Download the latest Releases from the GitHub page.

Setp 2:
Modify and edit the config.yaml file.

# Server
Here will be the port and address that the proxy listens on. For the local machine, it is http://127.0.0.1:9944/ For any host within the local network, it is http://*:9944/
# Node Pool
Node pool is where you should place the other nodes you need to proxy. Here is a simple example.

# Server
listen : "http://*:9944/"
# Node Pool
node:
  - name: "Private Node-0"
    url: "ws://192.168.0.185:9944"
Setp 3:
For Linux :
Grant execution permissions ：
chmod 775 Subspace.Agent
execution ：
./Subspace.Agent

For Winodws :
Listening on a local network requires running with administrator privileges, while listening on 127.0.0.1 can be run directly.
