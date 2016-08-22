library(shiny)

# Define UI for PET Design Space Browser application
shinyUI(fluidPage(

  #  Application title
  titlePanel("PET Design Space Browser"),
  #verbatimTextOutput("debug"),
  tabsetPanel(
    tabPanel("Pairs Plot",
      fluidRow(
        column(3,
          br(),
          wellPanel(
            h4("Plot Options"),
            selectInput("display",
                        "Display Variables:",
                        c(),
                        multiple = TRUE),
            conditionalPanel(
              condition = "input.autoRender == false",
              actionButton("renderPlot", "Render Plot"),
              br()
            ), hr(),
            checkboxInput("color", "Color Data", value = FALSE),
            conditionalPanel(
              condition = "input.color == true",
              # selectInput("colType", "Type:", choices = c("Max/Min", "Discrete"), selected = "Max/Min"),
              # conditionalPanel(
                # condition = "input.colType == 'Max/Min'",
                selectInput("colVarNum", "Colored Variable:", c()),
                radioButtons("radio", NULL, c("Maximize" = "max", "Minimize" = "min")),
                sliderInput("colSlider", NULL, min=0, max=1, value=c(0.3,0.7), step=0.1)
              # )
              # conditionalPanel(
              #   condition = "input.colType == 'Discrete'",
              #   selectInput("colVarFactor")
              # )
            ), hr(),
            h4("Info"), #br(),
            verbatimTextOutput("stats"),
            actionButton("updateStats", "Update"), br(), hr(),
            h4("Download"),
            downloadButton('exportData', 'Dataset'),
            paste("          "),
            downloadButton('exportPlot', 'Plot'), hr(),
            actionButton("resetOptions", "Reset to Default Options")
          )
        ),
        column(9,
          plotOutput("pairsPlot", height=700)
        )
      )
    ),
    tabPanel("Single Plot",
      fluidRow(
        column(3,
          br(),
          wellPanel(
            selectInput("xInput", "X-axis", c()),
            selectInput("yInput", "Y-Axis", c()),
            br(),
            p(strong("Adjust Sliders to Selection:")),
            actionButton("updateX", "X"),
            actionButton("updateY", "Y"),
            actionButton("updateBoth", "Both")
          )
        ),
        column(9,
          plotOutput("singlePlot", click = "plot_click", brush = "plot_brush", height=700)
        ),
        column(12,
          verbatimTextOutput("info")
        )
      )
    ),
    tabPanel("Data Table",
      wellPanel(
        fluidRow(
          br(), actionButton("updateDataTable", "Update Data Table"), br(), br()
        ),
        fluidRow(
          dataTableOutput(outputId="table")
        )
      )
    ),
    tabPanel("Options",
      fluidRow(
        column(3,
          br(),
          wellPanel(
            h4("Render Options"),
            checkboxInput("autoRender", "Automatically Rerender", value = TRUE),
            hr(),
            p(strong("Point Options:")),
            fluidRow(
              column(6, radioButtons("pointStyle", NULL, c("Normal" = 1,"Filled" = 19))),
              column(6, radioButtons("pointSize", NULL, c("Small" = 1, "Medium" = 1.5, "Large" = 2)))
            ),
            hr(),
            # actionButton("resetSettings", "Reset to Default Settings"),
            # hr(),
            h4("About"),
            p(strong("Version:"), "v1.1.2"),
            p(strong("Date:"), "12/29/2015"),
            p(strong("Developer:"), "Metamorph Software"),
            p(strong("Support:"), "tthomas@metamorphsoftware.com")
          )
        ),
      column(9))
    )
  ),
  h3("Filter Data:"),
  actionButton("resetSliders", "Reset Sliders"), br(), br(),
  uiOutput("enums"),
  uiOutput("sliders"),
  h3("Constants:"),
  uiOutput("constants")
  
  
)
)
