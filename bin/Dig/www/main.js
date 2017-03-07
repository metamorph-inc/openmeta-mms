var windowWidth = 0; 
var enter_trigger = 0;
var var_names = [];

/* Process window width on startup...currently not used */
$(document).on("shiny:connected", function(e) {  
  windowWidth = window.innerWidth; 
  Shiny.onInputChange("windowWidth", windowWidth);    
}); 

/* These next 2 functions process a change everytime the user adjusts window size */
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


/* This gets called when filter footer is opened...helpful as a startup condition */
Shiny.addCustomMessageHandler("update_widths", function(message) {
  update_slider_width();
  update_label_width();
})

/* Sends shiny the pixel lengths of each slider */
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

/* CURRENTLY NOT NEEDED
 * Sends shiny the pixel lengths of each slider label 
 * NOTE...any length > slider length will be 'saturated' at slider length
 */
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

/* Sends shiny a trigger whennever enter key is pressed */
$(document).on("keydown", function (e) {
  if(e.keyCode == 13){
    Shiny.onInputChange("last_key_pressed", enter_trigger++);
  }
});
