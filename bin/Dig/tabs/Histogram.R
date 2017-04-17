title <- "Histogram"
footer <- TRUE

ui <- function(id) {
  ns <- NS(id)
  
  fluidPage(
    br(),
    column(3,
      selectInput(ns("variable"), "Histogram Variable:", c())
    ),
    column(9,
      plotOutput(ns("plot"))
    )
  )
  
}

server <- function(input, output, session, data) {
  ns <- session$ns
  
  observe({
    selected <- isolate(input$variable)
    saved <- si_read(ns("variable"))
    if (!is.null(saved) && saved %in% c(data$pre$var_range(), "")) {
      selected <- si(ns("variable"), NULL)
    }
    updateSelectInput(session,
                      "variable",
                      choices = data$pre$var_range_nums_and_ints_list(),
                      selected = selected)
  })
  
  output$plot <- renderPlot({
    if(input$variable != "") {
      hist(data$Filtered()[[input$variable]],
           main = paste("Histogram of" , paste(input$variable)),
           xlab = paste(input$variable))
    }
  })
  
}