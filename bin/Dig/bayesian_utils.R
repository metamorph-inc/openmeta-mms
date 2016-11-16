#' Resamples the given dataset, using the specified distributionTypes and 
#' distributionParams.
#' 
#' @param data The dataframe to resample.
#' @param dataDirection A list containing, for each variable in data, whether 
#'   the variable is an input or output variable.
#' @param distributionTypes A list containing the desired distribution types of 
#'   the input data.  Should only contain entries for input variables.
#' @param distributionParams A list containing arameters for the distributions 
#'   of input data.  See \code{presuldfComp}'s documentation for the contents of each
#'   entry in this list.
#'   
#' @returns A list with two elements:  'dist': A list containing, for each
#'   variable, xOrig/yOrig (the chosen distribution for input variables, and the
#'   original distribution for output variables) and xResampled/yResampled (the
#'   resampled distribution). 'resampledData': a data frame containing the
#'   actual resampled data.
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
  
  distList = list()
  
  # Plot resampled samples
  for (var in names(data)) {
    if(dataDirection[[var]] == 'Input') {
      min = min(data_new[[var]])
      max = max(data_new[[var]])
      
      xpoint = seq(min, max, (max - min)/255)
      pdfAnalytical = pdfComp(distributionTypes[[var]], distributionParams[[var]], xpoint)
      pdfSample = density(data_new[[var]], n=256, from=min, to=max)
      
      result = list(xOrig = xpoint,
                    yOrig = pdfAnalytical,
                    xResampled = pdfSample[['x']],
                    yResampled = pdfSample[['y']])
      distList[[var]] = result
    } else if(dataDirection[[var]] == 'Output') {
      min = min(data_new[[var]])
      max = max(data_new[[var]])
      
      originalPdf = density(data[[var]], n=256, from=min, to=max)
      resampledPdf = density(data_new[[var]], n=256, from=min, to=max)
      
      result = list(xOrig = originalPdf[['x']],
                    yOrig = originalPdf[['y']],
                    xResampled = resampledPdf[['x']],
                    yResampled = resampledPdf[['y']])
      distList[[var]] = result
    } else {
      stop("Invalid data direction")
    }
  }
  
  outputList = list()
  outputList$dist = distList
  outputList$resampledData = data_new
  
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

#' Computes the kernel density probability density function and evaluates it at
#' the specified points.
#' 
#' @param data The data to compute the kernel density function over
#' @param pointsToEvaluate The points at which to evaulate the PDF
#'   
#' @return A vector containing the Y values of the probability distribution 
#'   function, evaluated at pointsToEvaluate
densityPdf = function(data, pointsToEvaluate) {
  min = min(data)
  max = max(data)
  pdf = density(data, n=2048, from=min, to=max)
  pdfFunction = approxfun(pdf$x, pdf$y, yleft=0, yright=0)
  
  return(sapply(pointsToEvaluate, pdfFunction))
}

#' Computes the kernel density cumulative density function and evaluates it at
#' the specified points.
#' 
#' @param data The data to compute the kernel density function over
#' @param pointsToEvaluate The points at which to evaulate the PDF
#'   
#' @return A vector containing the Y values of the cumulative distribution 
#'   function, evaluated at pointsToEvaluate
densityCdf = function(data, pointsToEvaluate) {
  min = min(data)
  max = max(data)
  pdf = density(data, n=2048, from=min, to=max)
  pdfFunction = approxfun(pdf$x, pdf$y, yleft=0, yright=0)
  integrationPoints = seq(min, max, (max-min)/2047)
  cdfPoints = sapply(integrationPoints, function(val) { integrate(pdfFunction, min(data), val, subdivisions=1000)$value })
  cdfFunction = approxfun(integrationPoints, cdfPoints, yleft=0, yright=1.0)
  
  cdfValues = cdfFunction(pointsToEvaluate)
  # Clamp values between 0 and 1, since our integration and approximation
  # occasionally gives us a slightly out-of-range value (numerical integration
  # isn't too accurate) and CDF values must be between 0 and 1.
  cdfValues[cdfValues < 0.0000001] = 0.0000001
  cdfValues[cdfValues > 0.9999999] = 0.9999999
  return(cdfValues)
}

#' Computes the kernel density inverse cumulative density function and evaluates
#' it at the specified points.
#' 
#' @param data The data to compute the kernel density function over
#' @param pointsToEvaluate The points at which to evaulate the PDF
#'   
#' @return A vector containing the Y values of the inverse cumulative
#'   distribution function, evaluated at pointsToEvaluate
densityInverseCdf = function(data, pointsToEvaluate) {
  min = min(data)
  max = max(data)
  pdf = density(data, n=2048, from=min, to=max)
  pdfFunction = approxfun(pdf$x, pdf$y, yleft=0, yright=0)
  cdfFunction = function(x) {
    if(x <= min) {
      return(0)
    } else if(x >= max) {
      return(1)
    } else {
      integrate(pdfFunction, min, x, subdivisions=1000)$value
    }
  }
  
  inverseCdfFunction = function(q) {
    uniroot(function(x) { cdfFunction(x) - q }, lower=min, upper=max)$root
  }
  
  return(sapply(pointsToEvaluate, inverseCdfFunction))
}

#' Converts xObs into standard normal variables using the distribution in x
x2u = function(x, xObs) {
  numberOfVariables = length(x)
  
  result = makeEmptyDataFrame(nrow(xObs))
  result["temp"] = NULL
  for (i in 1:numberOfVariables) {
    cdfValues = densityCdf(x[[i]], xObs[[i]])
    normalizedValues = qnorm(cdfValues, mean=0, sd=1)
    result[names(x)[i]] = normalizedValues
  }
  return(result)
}

#' Creates an empty (zero column) data frame with the specified number of rows.
#' 
#' @param numberOfRows Number of rows the data frame should initially contain
#'   
#' @return An empty data frame, with the specified number of rows and zero
#'   columns
makeEmptyDataFrame = function(numberOfRows) {
  result = data.frame(temp=1:numberOfRows)
  result["temp"] = NULL
  
  return(result)
}



#' Integrates under the curve specified by x and y, using linear interpolation
#' between points.
#' 
#' @param x numeric vector representing the x coordinates of the curve to
#'   integrate under
#' @param y numeric vector representing the y coordinates of the curve to
#'   integrate under (should be same length as x)
#' @param lowerBound The lower bound for integration
#' @param upperBound The upper bound for integration
#' @param yLeft For the original (pre-integration) curve, the value of y for
#'   values of x < min(x)
#' @param yRight For the original (pre-integration) curve, the value of y for
#'   values of x > max(x)
#' @param subdivisions Maximum number of subdivisions to use for integration
#'   
#' @return The area under the specified curve, between lowerBound and upperBound
integrateData = function(x, y, lowerBound, upperBound, yLeft = 0, yRight = 0, subdivisions = 1000) {
  curveFunction = approxfun(x, y, yleft=yLeft, yright=yRight)
  
  area = (integrate(curveFunction, lowerBound, upperBound, subdivisions=subdivisions))$value
}