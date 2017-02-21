title <- "Scratch"
footer <- TRUE

ui <- function() {
  
  fluidPage(
    br(),
    verbatimTextOutput("text")
  )
  
}

server <- function(input, output, session, data) {
  
  output$text <- renderPrint(data$Filters())
  
}