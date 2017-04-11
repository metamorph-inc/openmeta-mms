function convertmat
load output.mat;
ts = ans;
dataMatrix = [ts.Time, ts.Data];
dlmwrite('output.csv', dataMatrix, 'precision', '%.10f');
end
