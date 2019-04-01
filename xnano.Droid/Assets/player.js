var uri = "ws://" + location.host + "/vrws";
var ws = null;
var video = document.getElementById("video");
var state = "pa";
var time = 0.0;

function connect(){
   if(ws == null){
      ws = new WebSocket(uri);
      ws.onmessage = onMessage;
      ws.onopen = onOpen;
   }
}

function init(){

}

function update(){
   if(state == "pa"){
       video.pause();
   }
   else if(state == "pl"){
       video.play();
   }
   video.currentTime = time;
}
function getState(){
   ws.send("u");
}

function onMessage(event){
   if(event && event.data){
   var commands = event.data.split(":");
       state = commands[0];
       time = commands[1];
       update();
   }
}

function onOpen(event){
   getState();
}

