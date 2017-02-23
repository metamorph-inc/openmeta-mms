var windowWidth = 0; 
var $slider = document.querySelector("#filters > div > div:nth-child(2) > div:nth-child(1) > div");
var $labels;

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