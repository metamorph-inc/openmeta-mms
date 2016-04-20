/////////////////////////////////////////////////////////////////////
////                                                             ////
////  Simple NJL5501R Model	(+patient simulator)			     ////
////                                                             ////
////  SystemC Version: 2.3.0                                     ////
////  Author: Peter Volgyesi, MetaMorph, Inc.                    ////
////          pvolgyesi@metamorphsoftware.com                    ////
////                                                             ////
////   http://www.mouser.com/ds/2/294/NJL5501R_E-254983.pdf      ////
/////////////////////////////////////////////////////////////////////

#ifndef NJL5501R_H
#define NJL5501R_H

#include <systemc.h>

SC_MODULE(njl5501r) {

	typedef sc_uint<10>	analog_type; 

	// ports
	sc_in<analog_type>	red_led;
	sc_in<analog_type>	ir_led;
	sc_out<sc_uint<10>>	signal;

	static const double	RED_OFFSET;
	static const double	RED_AMPLITUDE;
	static const double	IR_OFFSET;
	static const double	IR_AMPLITUDE;
	static const double	HR;
	static const int	N_PATTERN;
	static const double	PATTERN[];

	SC_CTOR(njl5501r);

//private:
	sc_clock	clk;
	int		phase;

	void sig_gen();
};

#endif // NJL5501R_H