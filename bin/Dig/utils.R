# Custom Functions -----------------------------------------------------------

RemoveItemNumber <- function(factor) {sub("[0-9]+. ", "", factor)}

is.empty <- function(x) {
  is.null(x) || (is.list(x) && length(x) == 0)
}

BuildPet <- function(pet_config_filename) {
  pet_config <- fromJSON(pet_config_filename)
  dvs <- pet_config$drivers[[1]]$designVariables
  design_variable_names <- names(dvs)
  design_variables <- Map(function(item, name) {
    new_item <- list()
    new_item$name <- name
    if ("RangeMax" %in% names(item)) {
      new_item$type <- "Numeric"
    } else {
      new_item$type <- "Enumeration"
    }
    if("type" %in% names(item) && item$type == "enum") {
      new_item$selection <- paste0(unlist(item$items), collapse=",")
    } else {
      new_item$selection <- paste0(c(item$RangeMin, item$RangeMax),
                                   collapse=",")
    }
    new_item
  }, dvs, names(dvs))
  objective_names <- names(pet_config$drivers[[1]]$objectives)
  num_samples <- unlist(strsplit(as.character(pet_config$drivers[[1]]$details$Code),'='))[2]
  sampling_method <- pet_config$drivers[[1]]$details$DOEType
  generated_configuration_model <- pet_config$GeneratedConfigurationModel
  selected_configurations <- pet_config$SelectedConfigurations
  pet_name <- pet_config$PETName
  mga_name <- pet_config$MgaFilename
  
  pet <- list(sampling_method=sampling_method,
              num_samples=num_samples,
              pet_name=pet_name,
              mga_name=mga_name,
              generated_configuration_model=generated_configuration_model,
              selected_configurations=selected_configurations,
              design_variable_names=design_variable_names,
              design_variables=design_variables,
              objective_names=objective_names,
              pet_config=pet_config,
              pet_config_filename=pet_config_filename)
}

CleanUnits <- function(units) {
  if(is.null(units) || units == "") {
    units <- ""
  }
  else
  {
    units <- gsub("\\*\\*", "^", units) #replace Python '**' with '^'
    units <- gsub("inch", "in", units)  #replace 'inch' with 'in' since 'in' is a Python reserved word
    units <- gsub("yard", "yd", units)  #replace 'yard' with 'yd' since 'yd' is an OpenMDAO reserved word
  }
}

AddUnits <- function(name, units) {
  if(units == "") {
    name
  } else {
    name_with_units <- paste0(name," (",units,")")
  }
}
  
BuildVariables <- function(pet, var_names) {
  if(is.null(pet)) {
    variables <- lapply(var_names, function(var_name) {
      list(type="Unknown")
    })
  } else {
    pet_config <- pet$pet_config
    design_variable_names <- pet$design_variable_names
    objective_names <- pet$objective_names
    
    variables <- lapply(var_names, function(var_name) {
      if (var_name %in% design_variable_names) {
        type <- "Design Variable"
        units <- CleanUnits(pet_config$drivers[[1]]$designVariables[[var_name]]$units)
        name_with_units <- AddUnits(var_name, units)
      } else if(var_name %in% objective_names) {
        type <- "Objective"
        units <- CleanUnits(pet_config$drivers[[1]]$objectives[[var_name]]$units)
        name_with_units <- AddUnits(var_name, units)
      } else {
        type <- "Unknown"
        units <- NULL
        name_with_units <- var_name
      }
      list(type = type,
           units = units,
           name_with_units = name_with_units)
    })
  }
  names(variables) <- var_names
  variables
}

AddCategories <- function(vars) {
  var_list <- list()
  for(i in 1:length(vars)) {
    type <- vars[[i]]$type
    if(is.null(var_list[[type]])) {
      var_list[[type]] <- list()
    }
    var_list[[type]] <- c(var_list[[type]], names(vars)[i])
  }
  var_list
}
# RemoveUnits <- function(name_with_units) {
#   if(is.null(reverse_units) || !(name_with_units %in% names(reverse_units)))
#     name_with_units
#   else
#     reverse_units[[name_with_units]]
# }