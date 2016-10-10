#' Resamples the given dataset, using the specified distributionTypes and 
#' distributionParams.
#' 
#' @param data The dataframe to resample.
#' @param dataDirection A list containing, for each variable in data, whether
#'   the variable is an input or output variable.
#' @param distributionTypes A list containing the desired distribution types of
#'   the input data.  Should only contain entries for input variables.
#' @param distributionParams A list containing arameters for the distributions
#'   of input data.  See \code{pdfComp}'s documentation for the contents of each
#'   entry in this list.
#'
#' @returns A list containing, for each variable, xOrig/yOrig (the chosen
#'   distribution for input variables, and the original distribution for
#'   output variables) and xResampled/yResampled (the resampled distribution).
resampleData = function(data, dataDirection, distributionTypes, distributionParams) {
  numberOfVariables = ncol(data)
  
  # Compute weights of samples of input variables
  numberOfInputVariables = length(distributionTypes)
  numberOfSamples = nrow(data)
  
  likelihood = rep(1, numberOfSamples)
  for (var in names(data)) {
    if(dataDirection[[var]] == 'Input') {
      likelihoodTemp = pdfComp(distributionTypes[[var]], distributionParams[[var]], data[[var]])
      likelihood = likelihood * likelihoodTemp
    } else if(dataDirection[[var]] == 'Output') {
      # Intentionally left blank
    } else {
      stop("Invalid data direction")
    }
  }
  
  weight = likelihood / sum(likelihood)
  
  # Get resampled data
  resampleIndices = genDist(weight, numberOfSamples)
  data_new = data[resampleIndices, ]
  
  outputList = list()
  
  # Plot resampled samples
  for (var in names(data)) {
    if(dataDirection[[var]] == 'Input') {
      min = min(data_new[[var]])
      max = max(data_new[[var]])
      
      xpoint = seq(min, max, (max - min)/100)
      pdfAnalytical = pdfComp(distributionTypes[[var]], distributionParams[[var]], xpoint)
      pdfSample = density(data_new[[var]], n=100, from=min, to=max)
      
      result = list(xOrig = xpoint,
                    yOrig = pdfAnalytical,
                    xResampled = pdfSample[['x']],
                    yResampled = pdfSample[['y']])
      outputList[[var]] = result
    } else if(dataDirection[[var]] == 'Output') {
      min = min(data_new[[var]])
      max = max(data_new[[var]])
      
      originalPdf = density(data[[var]], n=100, from=min, to=max)
      resampledPdf = density(data_new[[var]], n=100, from=min, to=max)
      
      result = list(xOrig = originalPdf[['x']],
                    yOrig = originalPdf[['y']],
                    xResampled = resampledPdf[['x']],
                    yResampled = resampledPdf[['y']])
      outputList[[var]] = result
    } else {
      stop("Invalid data direction")
    }
  }
  
  return(outputList)
}

#' Gets values for the specified PDF at the points in 'data'.
#' 
#' @param distributionType The type of the desired probability distribution. Can
#'   be \code{'norm'}, for a normal distribution, or \code{'unif'}, for a
#'   uniform distribution.
#' @param distributionParams A list containing distribution parameters for the
#'   desired probability distribution.  Should contain mean and stdDev (standard
#'   deviation) for uniform distributions, and min and max for uniform
#'   distributions.
#' @param data The data values to sample the PDF at.
#' 
#' @return A vector containing the PDF value at each element in \code(data).
pdfComp = function(distributionType, distributionParams, data) {
  if(distributionType == "norm") {
    return(dnorm(data, mean=distributionParams[["mean"]], sd=distributionParams[["stdDev"]]))
  } else if(distributionType == "unif") {
    return(dunif(data, min=distributionParams[["min"]], max=distributionParams[["max"]]))
  } else {
    stop("Invalid distribution type")
  }
}

# Generates an array of random numbers of length 'length' according to
# the discrete probability distribution 'dist'
genDist = function(distribution, length) {
  sample.int(length(distribution), size=length, replace=TRUE, prob=distribution)
}