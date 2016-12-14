library(shiny)
library(shinyjs)

# Define UI for Visualizer
shinyUI(fluidPage(
  #  Application title
  titlePanel("Visualizer"),
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
              h4("Data Coloring"),
              selectInput("colType", "Type:", choices = c("None", "Max/Min", "Discrete", "Highlighted", "Ranked"), selected = "None"),
              conditionalPanel(
                condition = "input.colType == 'Max/Min'",
                selectInput("colVarNum", "Colored Variable:", c()),
                radioButtons("radio", NULL, c("Maximize" = "max", "Minimize" = "min"), selected = "max"),
                sliderInput("colSlider", NULL, min=0, max=1, value = c(0.3, 0.7), step=0.1)
              ),
              conditionalPanel(
                condition = "input.colType == 'Discrete'",
                selectInput("colVarFactor", "Colored Variable:", c()),
                htmlOutput("colorLegend")
              )
              
            ,  hr(),
            h4("Info"), #br(),
            verbatimTextOutput("stats"),
            conditionalPanel(condition = "input.autoInfo == false",
                            actionButton("updateStats", "Update"),
                            br()),  hr(),
            h4("Download"),
            downloadButton('exportData', 'Dataset'), 
            paste("          "),
            downloadButton('exportPlot', 'Plot'), hr(),
            actionButton("resetOptions", "Reset to Default Options")
          )
        ),
        column(9,
            uiOutput("displayError"),   
            uiOutput("filterError"), 
            plotOutput("pairsPlot", dblclick = "pairs_click", height = 700)
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
            p(strong("Adjust Sliders to Selection")),
            actionButton("updateX", "X"),
            actionButton("updateY", "Y"),
            actionButton("updateBoth", "Both"),
            br(), br(),
            #p(strong("Highlight Selection")),
            bootstrapPage(
              actionButton("highlightData", "Highlight Selection", class = "btn btn-primary")
            )
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
      br(),
      radioButtons("activateRanking", "Output Preference", choices = c("Unranked", "TOPSIS", "Simple Metric w/ TxFx"), selected = "Unranked"),
      wellPanel(
        style = "overflow-x:auto",
        DT::dataTableOutput("dataTable"),
        downloadButton("exportPoints", "Export Selected Points"), 
        actionButton("colorRanked", "Color by Selected Rows")
        #checkboxInput("transpose", "Transpose Table", value = FALSE)
        
      ),
      conditionalPanel(condition = "input.activateRanking != 'Unranked'",
        wellPanel(
          conditionalPanel(condition = "input.autoRanking == false",
            actionButton("applyRanking", "Apply Ranking"),
            br(), br()
          ),
          fluidRow(
            column(4, 
              selectInput("weightMetrics",
                          "Ranking Metrics:",
                          c(),
                          multiple = TRUE),
              actionButton("clearMetrics", "Clear Metrics")
            )
          ),
          conditionalPanel(condition = "input.weightMetrics != null",
                           hr()),
          fluidRow(
            column(3, strong(h4("Variable Name"))),
            column(2, strong(h4("Ranking Mode"))),
            conditionalPanel(condition = "input.activateRanking == 'TOPSIS'",
              column(7, strong(h4("Weight Amount")))),
            conditionalPanel(condition = "input.activateRanking == 'Simple Metric w/ TxFx'",
              column(3, strong(h4("Weight Amount"))),
              column(4, strong(h4("Transfer Function"))))
          ),
          uiOutput("rankings")#, 
          #br(), hr(), 
          #br()
        )
      )
    ), 
    tabPanel("PET Refinement",
      br(),
      conditionalPanel(condition = "output.petConfigPresent == true",
        wellPanel(
          h4("Driver Configuration"),
          fluidRow(
            column(6,
                   h5(strong('Original Driver Settings: ')),
                   textOutput("originalDriverSettings"))
          ), br(),
          uiOutput("petDriverConfig"), hr(),
          fluidRow(
            column(12,
                   h4("Design Configurations"),
                   h5(strong("Generated Configuration Model: ")),
                   textOutput("generatedConfigurationModelText"),
                   fluidRow(
                     column(2),
                     column(1, h5(strong('Original'))),
                     column(2, h5(strong("Selection:"))),
                     column(1, h5(strong('Refined'))),
                     column(2, h5(strong("Selection:"))),
                     column(4, h5(strong("New Selection:")))
                   ), br(),
                   uiOutput("original_configuration_ranges")
            )
          ), hr(),
          conditionalPanel(condition = "output.numericDesignVariables == true",
            fluidRow(
              column(12,
                     h4("Numeric Ranges"),
                     fluidRow(
                       column(2, h5(strong("Variable Name:"))),
                       column(1, actionButton('applyAllOriginalNumeric', 'Original')),
                       column(1, h5(strong("Minimum:"))),
                       column(1, h5(strong("Maximum:"))),
                       column(1, actionButton('applyAllRefinedNumeric', 'Refined')),
                       column(1, h5(strong("Minimum:"))),
                       column(1, h5(strong("Maximum:"))),
                       column(2, h5(strong("New Minimum:"))),
                       column(2, h5(strong("New Maximum:")))
                     ), br(),
                     uiOutput("original_numeric_ranges")
              )
            )
          ),
          conditionalPanel(condition = "output.enumerationDesignVariables == true",
            fluidRow(
              column(12,
                     h4("Enumerated Ranges"),
                     fluidRow(
                       column(2, h5(strong("Variable Name:"))),
                       column(1, actionButton('applyAllOriginalEnum', 'Original')),
                       column(2, h5(strong("Selection:"))),
                       column(1, actionButton('applyAllRefinedEnum', 'Refined')),
                       column(2, h5(strong("Selection:"))),
                       column(4, h5(strong("New Selection:")))
                     ), br(),
                     uiOutput("original_enumeration_ranges")
              )
            )
          ),
          # , fluidRow(
          #   h4("Factor Statistics", align = "center"),
          #   uiOutput("factor_ranges")
          # )
          hr(),
          fluidRow(
            column(6,
            # conditionalPanel(condition = "input.autoRange == false",
            #                            actionButton("updateRanges", "Update Ranges"), br(), br()),
            h4("PET Details"),
            uiOutput("petRename"),
            # downloadButton('downloadRanges', 'Download \'pet_config_refined.json\''), br(), br(),
            actionButton('runRanges', 'Execute New PET'), br())
          )
        )
      ),
      conditionalPanel(condition = "output.petConfigPresent == false",
        verbatimTextOutput("noPetConfigMessage")
      )
    ),
    tabPanel("Uncertainty Quantification", 
      br(),
      tabsetPanel(
        tabPanel("Weighting",
          br(),
          fluidRow(
            column(3,
              checkboxInput('bayesianDesignConfigsPresent', "Multiple Design Configurations Present", value = F)
            )
          ),
          fluidRow(
            conditionalPanel(condition = 'input.bayesianDesignConfigsPresent == true',
              column(3,
                selectInput('bayesianDesignConfigVar', "Design Configuration Identifier",
                  choices = c(),
                  multiple = F)
              )
            ),
            conditionalPanel(condition = 'input.bayesianDesignConfigsPresent == true & input.bayesianDesignConfigVar != null',
              column(3,
                selectInput('bayesianDesignConfigChoice', "Selection",
                  choices = c(),
                  multiple = F)
              )
            )
          ),
          fluidRow(
            column(3,
              checkboxInput('bayesianDisplayAll', "Display All Variables", value = T),
              conditionalPanel(condition = 'input.bayesianDisplayAll == false', 
                selectInput(
                  'bayesianDisplayVars',
                  "Bayesian Variables",
                  choices = c(),
                  multiple = T
                )
              )
            )
          ),
          fluidRow(
            column(6, 
              wellPanel(h4("Variable Configuration"),
                uiOutput("bayesianUI"), br()#, height = 200)
              )
            ),
            column(6,
              wellPanel(h4("Variable Plots"), br(),
                uiOutput("bayesianPlots")
              )
            )
          ),
          actionButton('runFUQ', 'Run Forward UQ')
        ),
        # --------REMOVE THIS SECTION---------------
        # tabPanel("Forward UQ",
        #   br(),
        #   h4("Constraints:"),
        #   fluidRow(
        #     column(6,
        #       wellPanel(
        #         fluidRow(
        #           column(2, h5(strong("Enable"))),
        #           column(6, h5(strong("Variable:"))),
        #           column(4, h5(strong("Value:")))
        #         ),
        #         uiOutput("fuqConstraintsUI")
        #       )
        #     ), column(6)
        #   ), br(),
        #   # actionButton('runFUQ', 'Run Forward UQ'),
        #   br(),
        #   hr(),
        #   uiOutput("fuqPlots")
        # ),
        tabPanel("Design Ranking",
          br(),
          actionButton('runProbability', 'Compute Probabilities'),
          br(), br(),
          h4("Weights"),
          wellPanel(
            fluidRow(
              column(1, h5(strong("ID:"))),
              column(1, h5(strong("Source:"))),
              column(3, h5(strong("Description:"))),
              column(4, h5(strong("Impact:"))),
              column(3, h5(strong("Weight:")))
            ), br(),
            uiOutput("probabilityWeightUI")
          ),
          h4("Rankings"),
          wellPanel(
            DT::dataTableOutput("probabilityTable")
          )
        ),
        id = "bayesianTabset"
      ),
      conditionalPanel("output.displayQueries",
        hr(),
        h4("Probability Queries:"),
        wellPanel(
          fluidRow(
            column(1, actionButton('addProbability', 'Add')),
            column(3, h5(strong("Variable Name:"))),
            column(2, h5(strong("Direction:"))),
            column(2, h5(strong("Threshold:"))),
            column(2, h5(strong("Value:"))),
            column(2)
          ), br(),
          tags$div(id = 'probabilityUI'),
          hr(),
          actionButton('runProbabilityQueries', 'Evaluate')
        )
      )
    ),
    tabPanel("Options",
      fluidRow(
        column(6,
          br(),
          wellPanel(
            tags$div(title = "Return to default settings.",
                     actionButton("resetSettings", "Reset All Settings") 
            ),
            hr(),
            h4("Data Processing Options"),
            tags$div(title = "Removes data points that have missing attributes.",
                     checkboxInput("removeMissing", "Remove Incomplete Rows", value = TRUE)), 
            fluidRow(
              column(4, 
                tags$div(title = "Removes data points outside of a set number of standard deviations from the mean.", 
                  checkboxInput("removeOutliers", "Remove Outliers", value = FALSE))),
              conditionalPanel("input.removeOutliers == '1'", 
                column(8, 
                  tags$div(title = "Number of standard deviations to filter data by.", 
                    sliderInput("numDevs", HTML("&sigma;:"), min = 1, max = 11, step = 0.1, value = 6))))
            ),
            fluidRow(
              column(4, 
                     tags$div(title = "Rounds data in all tables to a set number of decimal places", 
                              checkboxInput("roundTables", "Decimals Displayed", value = FALSE))),
              conditionalPanel("input.roundTables == '1'", 
                               column(8, 
                                      tags$div(title = "Maximum number of decimals to show in data tables.", 
                                               sliderInput("numDecimals", "Decimal Places", min = 1, max = 11, step = 1, value = 4))))
            ),
            tags$div(title = "Sticky Filters try to preserve their settings when removing/adding outliers or missing data rows.", 
                     checkboxInput("stickyFilters", "Sticky Filters", value = TRUE)),
            hr(),
            
            h4("Render Options"),
            tags$div(title = "Pairs plot will automatically update.",
                     checkboxInput("autoRender", "Automatically Rerender Plot", value = TRUE)),
            tags$div(title = "Allow trendline to be inserted into plot.",
                     checkboxInput("trendLines", "Overlay Trendline(s)", value = FALSE)),
            tags$div(title = "Shows the upper panel in the Pairs Plot.",
                     checkboxInput("upperPanel", "Display Upper Panel", value = FALSE)),
            strong("Data Point Style"),
            fluidRow(
              column(4, tags$div(title = "Normal: cheerios, Filled: dots.", 
                                 radioButtons("pointStyle", NULL, c("Normal" = 1,"Filled" = 19)))),
              column(8, tags$div(title = "Size of data points.",
                                 radioButtons("pointSize", NULL, c("Small" = 1, "Medium" = 1.5, "Large" = 2))))
            ),
            hr(),
            
            h4("Automatic Refresh"),
            tags$div(title = "Automatically updates info pane on pairs plot tab.",
                     checkboxInput("autoInfo", "Info Pane", value = TRUE)),
            tags$div(title = "Automatically updates ranking settings on Data Table tab.",
                     checkboxInput("autoRanking", "Data Ranking", value = TRUE)),
            tags$div(title = "Automatically updates Ranges Tab.",
                     checkboxInput("autoRange", "Ranges", value = TRUE))
          )
        ),
        column(6,
          br(),
          wellPanel(
            h4("Color Options"),
            fluidRow(
              column(4, tags$div(title = "Default color of data points.",
                                 colourInput("normColor", "Normal", "black")))
            ),
            fluidRow(
              column(4, tags$div(title = "Color of 'worst' data points.", 
                                 colourInput("maxColor", "Worst", "#E74C3C"))),
              column(4, tags$div(title = "Color of 'in between' data points.", 
                                 colourInput("midColor", "In Between", "#F1C40F"))),
              column(4, tags$div(title = "Color of 'best' data points.", 
                                 colourInput("minColor", "Best", "#2ECC71")))
            ),
            fluidRow(
              column(4, tags$div(title = "Color of highlighted data points.",
                                 colourInput("highlightColor", "Highlighted", "#377EB8")))
            ),
            fluidRow(
              column(4, tags$div(title = "Color of ranked data points.",
                                 colourInput("rankColor", "Ranked", "#D13ABA")))
            ),
            fluidRow(
              column(4, tags$div(title = "Color of ranked data points.",
                                 colourInput("bayHistColor", "Histogram", "wheat"))),
              column(4, tags$div(title = "Color of ranked data points.",
                                 colourInput("bayOrigColor", "Original", "#000000"))),
              column(4, tags$div(title = "Color of ranked data points.",
                                 colourInput("bayResampledColor", "Resampled", "#5CC85C")))
            )
          )
        ),
        column(6,
          br(),
          wellPanel(
            h4("Session Options"),
            strong("Save Session"),
            textInput("sessionName", NULL, placeholder = "Enter a filename..."),
            tags$div(title = "Download current settings of visualizer.",
                     downloadButton("exportSession", "Download")),
            br(), br(),
            strong("Load Session"), br(),
            tags$div(title = "Load a saved session.",
                     actionButton('importSession', 'Choose File')),
            textInput("loadSessionName", NULL, placeholder = "Enter a file path to open...")
          )
        )
      ),
      fluidRow(
        column(6,
          wellPanel(
            h4("About"),
            p(strong("Version:"), "v1.7.0"),
            p(strong("Date:"), "12/9/2016"),
            p(strong("Developer:"), "Metamorph Software"),
            p(strong("Support:"), "tthomas@metamorphsoftware.com")
          )
        )
      )
    ),
    id = "inTabset"
  ),
  conditionalPanel("output.displayFilters",
    hr(),
    h3("Filter Data:"),
    wellPanel(
      tags$div(title = "Activate to show filters for all dataset variables.",
               checkboxInput("viewAllFilters", "View All Filters", value = TRUE)),
      tags$div(title = "Return visible sliders to default state.",
               actionButton("resetSliders", "Reset Visible Filters")),
      hr(),
      uiOutput("filters")
    ),
    conditionalPanel("output.constantsPresent",
      h3("Constants:"),
      uiOutput("constants")
    )
  )
))
