# Custom Functions -----------------------------------------------------------

RemoveItemNumber <- function(factor) {sub("[0-9]+. ", "", factor)}

is.empty <- function(x) {is.null(unlist(x))}

BuildPet <- function(pet_config_filename) {
  # pet_config <- fromJSON(pet_config_filename)
  pet_config <- fromJSON(pet_config_filename, simplifyDataFrame=FALSE)
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
  constraint_names <- names(pet_config$drivers[[1]]$constraints)
  intermediate_variable_names <- names(pet_config$drivers[[1]]$intermediateVariables)
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
              constraint_names=constraint_names,
              intermediate_variable_names=intermediate_variable_names,
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
      list(type = "Unknown",
           units = "",
           name_with_units = var_name)
    })
  } else {
    pet_config <- pet$pet_config
    design_variable_names <- pet$design_variable_names
    objective_names <- pet$objective_names
    constraint_names <- pet$constraint_names
    intermediate_variable_names <- pet$intermediate_variable_names
    
    variables <- lapply(var_names, function(var_name) {
      if (var_name %in% design_variable_names) {
        type <- "Design Variable"
        units <- CleanUnits(pet_config$drivers[[1]]$designVariables[[var_name]]$units)
        name_with_units <- AddUnits(var_name, units)
      } else if(var_name %in% objective_names) {
        type <- "Objective"
        units <- CleanUnits(pet_config$drivers[[1]]$objectives[[var_name]]$units)
        name_with_units <- AddUnits(var_name, units)
      } else if(var_name %in% constraint_names) {
        type <- "Constraint"
        units <- NULL
        name_with_units <- var_name
      } else if(var_name %in% intermediate_variable_names) {
        type <- "Intermediate Variable"
        units <- CleanUnits(pet_config$drivers[[1]]$intermediateVariables[[var_name]]$units)
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

GetConfigFolders <- function(current_dir) {
  # print(paste0("GetConfigFolders(", current_dir, ")"))
  metadata_filename <- file.path(current_dir,"metadata.json")
  folders <- list()
  if(file.exists(metadata_filename)) {
    datasets <- fromJSON(metadata_filename)$SourceDatasets
    if(!is.null(datasets$Folders)) {
      direct_folders <- datasets$Folders[datasets$Kind == 0]
      indirect_folders <- datasets$Folders[datasets$Kind != 0]
      indirect_folders <- lapply(indirect_folders, function(folder) {
        normalizePath(file.path(current_dir, '..', folder))
      })
      folders <- c(folders,
                   unlist(direct_folders),
                   unlist(lapply(indirect_folders, GetConfigFolders)))
    } else {
      print("No folders found.")
    }
  }
  folders
}

FindGUIDFolders <- function(results_dir, config_folders) {
  guid_folders <- list()
  if(length(config_folders) > 0) {
    for (i in 1:length(config_folders)) {
      artifacts_folder <- suppressWarnings(
        normalizePath(file.path(results_dir,
                                dirname(config_folders[[i]]),
                                'artifacts')))
      guids <- list.files(artifacts_folder)
      if(length(guids) != 0) {
        # print(paste0(artifacts_folder,": ",length(guids)," points."))
        for (j in 1:length(guids)) {
          guid <- guids[[j]]
          guid_folders[[guid]] <- normalizePath(file.path(artifacts_folder,
                                                          guid))
        }
      }
    }
  }
  guid_folders
}

# ---- Design Tree supporting functions -------
compare_node <- function(cur, fil) {
  return ((is.null(cur$Selected) || cur$Selected == FALSE || fil$Selected == TRUE) &&
          (is.null(cur$Children) || compare_children(cur$Children, fil$Children)))
}

compare_children <- function(cur_children, fil_children) {
  result <- TRUE
  for(i in 1:length(cur_children)) {
    for(j in 1:length(fil_children)) {
      if(cur_children[[i]]$Name == fil_children[[j]]$Name) {
        result <- result && compare_node(cur_children[[i]], fil_children[[j]])
      }
    }
  }
  result
}

SelectAllComponents <- function(node) {
  if(node[['Type']] == "Component") {
    node[['Selected']] <- TRUE
  } else {
    node[['Children']] <- lapply(node[['Children']], SelectAllComponents)
  }
  node
}

  
