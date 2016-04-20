/////////////////////////////////////////////////////////////////////
////                                                             ////
////  Simple NJL5501R Model	(+patient simulator)			     ////
////                                                             ////
////  SystemC Version: 2.3.0                                     ////
////  Author: Peter Volgyesi, MetaMorph, Inc.                    ////
////          pvolgyesi@metamorphsoftware.com                    ////
////                                                             ////
////                                                             ////
/////////////////////////////////////////////////////////////////////
#include "njl5501r.h"

#define PI 3.14159265358979323846

const double	njl5501r::RED_OFFSET = 0.5;
const double	njl5501r::RED_AMPLITUDE = 0.5;
const double	njl5501r::IR_OFFSET = 0.8;
const double	njl5501r::IR_AMPLITUDE = 0.2;
const double	njl5501r::HR = 60.0;
const double	njl5501r::PATTERN[] = {0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, -0.1, -0.2, 0.1, 0.5, 0.9, 0.9, 0.9, 0.9, 0.4, 0.05, 0.0, 0.0};
const int		njl5501r::N_PATTERN = sizeof(PATTERN) / sizeof(double);

void njl5501r::sig_gen()
{
	analog_type out;

	out = (ir_led.read() * (IR_OFFSET + IR_AMPLITUDE * PATTERN[phase])) +
		  (red_led.read() * (RED_OFFSET + RED_AMPLITUDE * PATTERN[phase]));
	int tmp = (ir_led.read() * (IR_OFFSET + IR_AMPLITUDE * PATTERN[phase])) +
		  (red_led.read() * (RED_OFFSET + RED_AMPLITUDE * PATTERN[phase]));
	phase = (phase + 1) % N_PATTERN;
	signal.write(out);
}


njl5501r::njl5501r(sc_module_name mname) : sc_module(mname),
	clk("sigclock", (1.0e3/(HR/60)/N_PATTERN), SC_US), phase(0.0) 
	// NOTE: TODO:      SC_MS should be here ---^ but simulation would be slow
{
		SC_METHOD(sig_gen);
			sensitive << clk;
}