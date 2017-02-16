title <- "Histogram"

ui <- function() {
  
  fluidPage(
    br(),
    column(3,
      selectInput("sandboxVar", "Histogram Variable:", c())
    ),
    column(9,
      plotOutput("sandboxPlot")
    )
  )
  
}

server <- function(input, output, session, data) {
  
  varNames <- names(data$raw)
  varClass <- sapply(data$raw,class)
  varNums <- varNames[varClass != "factor"]
  
  updateSelectInput(session, "sandboxVar", choices = varNums, selected = varNums[1])
  
  output$sandboxPlot <- renderPlot({
    if(input$sandboxVar != "")
      hist(data$raw[[input$sandboxVar]],
           main = paste("Histogram of" , paste(input$sandboxVar)),
           xlab = paste(input$sandboxVar))
  })
  
}