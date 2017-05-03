function [ handle ] = CreateOrOverwriteModel( modelName )
%UNTITLED Summary of this function goes here
%   Detailed explanation goes here
if exist(modelName,'file') == 4
    % If it does then check whether it's open
    if bdIsLoaded(modelName)
        % If it is then close it (without saving!)
        close_system(modelName,0)
    end
    % delete the file
    if verLessThan('matlab', '8.0')
        delete([modelName, '.mdl']);
    else
        delete([modelName,'.slx']);
    end
end

handle = new_system(modelName);

end

