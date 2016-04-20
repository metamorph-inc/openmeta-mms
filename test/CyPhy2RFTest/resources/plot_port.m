
%% prepare simulation folder
Sim_Path = '.';
Sim_CSX = 'openEMS_input.xml';

%% Setup the simulation
physical_constants;
unit = 1e-3; % all lengths in mm

f0 = 1e9; % center frequency
lambda0 = c0/f0;

f_stop = 1.5e9; % 20 dB corner frequency
lambda_min = c0/f_stop

mesh_res_phantom = 2.5

dipole_length = 0.46*lambda0/unit
disp(['Lambda-half dipole length: ' num2str(dipole_length) 'mm'])


CSX = InitCSX();
feed.R = 50; % feed resistance
[CSX port] = AddLumpedPort(CSX, 100, 1, feed.R, [-0.1 -0.1 -mesh_res_phantom/2], [0.1 0.1 +mesh_res_phantom/2], [0 0 1], true);

%% Postprocessing and plots
freq = linspace(500e6, 1500e6, 501 );
port = calcPort(port, Sim_Path, freq);

s11 = port.uf.ref./port.uf.inc;
Zin = port.uf.tot./port.if.tot;

Pin = real(0.5*port.uf.tot.*conj(port.if.tot));
Pin_f0 = interp1(freq, Pin, f0)

% plot feed point impedance
figure
plot( freq/1e6, real(Zin), 'k-', 'Linewidth', 2 );
hold on
grid on
plot( freq/1e6, imag(Zin), 'r--', 'Linewidth', 2 );
title( 'Feed point impedance' );
xlabel( 'Frequency [MHz]' );
ylabel( 'Impedance Z_{in} [Ohm]' );
legend( 'Real', 'Imag' );

% plot reflection coefficient S11
figure
plot( freq/1e6, 20*log10(abs(s11)), 'k-', 'Linewidth', 2 );
grid on
title( 'Reflection coefficient' );
xlabel( 'Frequency [MHz]' );
ylabel( 'S_{11} [dB]' );

% plot P_in
figure
plot( freq/1e6, 20*log10(Pin), 'k-', 'Linewidth', 2 );
grid on
title( 'Accepted power' );
xlabel( 'Frequency [MHz]' );
ylabel( 'P_in [dB]' );

% function print_vector(vec, name)
%     fprintf(1,'public string[] %s = new string[] \n{', name)
%     for i = 1 : length(vec)
%         if mod((i-1),8) == 0
%            fprintf(1,'\n\t')
%         end
%         fprintf(1,'%12s', sprintf('"%.4e"', vec(i)));
%         if i ~= length(vec)
%            fprintf(1,', ')
%         end
%     end
%    fprintf(1,'\n};\n')
% endfunction

% print_vector(Pin, 'Pin')
% print_vector(real(Zin), 'Zin_real')
% print_vector(imag(Zin), 'Zin_imag')
% print_vector(real(s11), 'S11_real')
% print_vector(imag(s11), 'S11_imag')

pause(20)

