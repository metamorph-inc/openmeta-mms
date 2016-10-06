source("bayesian_utils.R")

# Load data
data_all = read.csv("WindTurbineSim.csv")

# Get subset of data for training
data = data_all[1:4500, ]

numberOfVariables = ncol(data)

dataDirections = list("modelica.jturbine"="input",
                      "modelica.ratio"="input",
                      "modelica.rho"="input",
                      "tl_peakPowerOutput.output"="output",
                      "tl_integratedEnergy.output"="output"
                      )

# Set up distribution types and parameters
distributionTypes = list("modelica.jturbine"="norm",
                         "modelica.ratio"="unif",
                         "modelica.rho"="norm")

distributionParams = list("modelica.jturbine"=list(mean=mean(data[,"modelica.jturbine"]),
                                                   stdDev=0.2*abs(mean(data[,"modelica.jturbine"]))),
                          "modelica.ratio"=list(min=min(data[,"modelica.ratio"]),
                                                max=max(data[,"modelica.ratio"])),
                          "modelica.rho"=list(mean=mean(data[,"modelica.rho"]),
                                              stdDev=0.1*abs(mean(data[,"modelica.rho"]))))

results = resampleData(data, dataDirections, distributionTypes, distributionParams)

for(i in 1:numberOfVariables) {
  plot(results[[i]][["xOrig"]], results[[i]][["yOrig"]], type='l', col="red", main=names(data)[[i]])
  lines(results[[i]][["xResampled"]], results[[i]][["yResampled"]], col="green")
}