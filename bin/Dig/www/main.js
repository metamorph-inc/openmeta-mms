var windowWidth = 0; 
var enter_trigger = 0;
var var_names = [];

$(document).on("shiny:connected", function(e) {  
  windowWidth = window.innerWidth; 
  Shiny.onInputChange("windowWidth", windowWidth);    
}); 

// Shiny.addCustomMessageHandler("send_vars", function(message){
//   var_names = message;
// });

$(window).resize(function() {
  if(this.resizeTO) clearTimeout(this.resizeTO);
    this.resizeTO = setTimeout(function() {
      $(this).trigger("resizeEnd");
    }, 500);
});

$(window).bind("resizeEnd", function() {
  // windowWidth = window.innerWidth;  
  // Shiny.onInputChange("windowWidth", windowWidth); 
  console.log("resize occured");

  update_slider_width();
  update_label_width();
});

Shiny.addCustomMessageHandler("update_widths", function(message) {

  update_slider_width();
  update_label_width();
})

function update_slider_width() {
  var $sliders = document.querySelector("#filters").querySelectorAll("div.form-group.shiny-input-container");

  var sliderWidth = 0;

  if($sliders.length > 0){
    for(var i = 0; i < $sliders.length; i++) {
      if($sliders[i].offsetWidth > sliderWidth)
        sliderWidth = $sliders[i].offsetWidth;
    }

    console.log("sliderWidth");
    console.log(sliderWidth);

    Shiny.onInputChange("sliderWidth", sliderWidth);
  }
}

function update_label_width() {
  var labelWidth = [];

  var $labels = document.querySelector("#filters").querySelectorAll("label.control-label");

  if($labels.length > 0){
    for(var i = 0; i < $labels.length; i++){
      labelWidth.push($labels[i].offsetWidth)
    }

    console.log("labelWidth");
    console.log(labelWidth);

    Shiny.onInputChange("labelWidth", labelWidth);
  }
}

// Triggers when an enter key is pressed
$(document).on("keydown", function (e) {
  if(e.keyCode == 13){
    Shiny.onInputChange("last_key_pressed", enter_trigger++);
  }
});
