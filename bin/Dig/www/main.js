var windowWidth = 0; 
var $slider = document.querySelector("#filters > div > div:nth-child(2) > div:nth-child(1) > div");
var $labels;
var enter_trigger = 0;

$(document).on("shiny:connected", function(e) {  
windowWidth = window.innerWidth; 
Shiny.onInputChange("windowWidth", windowWidth);    
});   

$(window).resize(function() {
if(this.resizeTO) clearTimeout(this.resizeTO);
this.resizeTO = setTimeout(function() {
$(this).trigger("resizeEnd");
}, 500);
});

$(window).bind("resizeEnd", function() {
windowWidth = window.innerWidth;  
Shiny.onInputChange("windowWidth", windowWidth); 
});

$(window).bind("resizeEnd", function() {
Shiny.onInputChange("dimension", [window.innerWidth, window.innerHeight]); 
});

Shiny.addCustomMessageHandler("sliderSize", function(message) {
  $slider = document.querySelector("#filters > div > div:nth-child(2) > div:nth-child(1) > div");
  var sliderWidth = $slider.offsetWidth; // all sliders are the same size

  Shiny.onInputChange("sliderWidth", sliderWidth);
});

Shiny.addCustomMessageHandler("labelSize", function(message) {
  var indices = message.length; // length of var names
  var labelWidth = [];

  $labels = document.querySelector("#filters").querySelectorAll("label");
  for(var i = 0; i < indices; i++){
    labelWidth.push($labels[i].offsetWidth)
  }

  Shiny.onInputChange("labelWidth", labelWidth);
});

// Triggers when an enter key is pressed
$(document).on("keydown", function (e) {
  if(e.keyCode == 13){
    Shiny.onInputChange("last_key_pressed", enter_trigger++);
  }
});
