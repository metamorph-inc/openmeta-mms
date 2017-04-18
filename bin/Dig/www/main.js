var window_width = 0; 
var enter_trigger = 0;
var var_names = [];

/* Process window width on startup...currently not used */
$(document).on("shiny:connected", function(e) {  
  window_width = window.innerWidth; 
  Shiny.onInputChange("window_width", window_width);    
}); 

/* These next 2 functions process a change everytime the user adjusts window size */
$(window).resize(function() {
  if(this.resizeTO) clearTimeout(this.resizeTO);
    this.resizeTO = setTimeout(function() {
      $(this).trigger("resizeEnd");
    }, 500);
});

$(window).bind("resizeEnd", function() {
  console.log("resize occured");
  update_slider_width();
});

/* This gets called when filter footer is opened...helpful as a startup condition */
Shiny.addCustomMessageHandler("update_widths", function(message) {
  update_slider_width();
  update_label_width();
})

/* Sends shiny the pixel lengths of each slider */
function update_slider_width() {
  var $sliders = document.querySelector("#filters").querySelectorAll("div.form-group.shiny-input-container");

  var slider_width = 0;

  if($sliders.length > 0){
    for(var i = 0; i < $sliders.length; i++) {
      if($sliders[i].offsetWidth > slider_width)
        slider_width = $sliders[i].offsetWidth;
    }

    console.log("slider_width");
    console.log(slider_width);

    Shiny.onInputChange("slider_width", slider_width);
  }
}

/* Sends shiny a trigger whennever enter key is pressed */
$(document).on("keydown", function (e) {
  if(e.keyCode == 13){
    Shiny.onInputChange("last_key_pressed", enter_trigger++);
  }
});
