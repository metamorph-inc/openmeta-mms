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

server <- function(input, output, session, data, info) {
  
  varNames <- names(data)
  varClass <- sapply(data,class)
  varNums <- varNames[varClass != "factor"]
  
  updateSelectInput(session, "sandboxVar", choices = varNums, selected = varNums[1])
  
  output$sandboxPlot <- renderPlot({
    if(input$sandboxVar != "")
      hist(data[[input$sandboxVar]],
           main = paste("Histogram of" , paste(input$sandboxVar)),
           xlab = paste(input$sandboxVar))
  })
  
}