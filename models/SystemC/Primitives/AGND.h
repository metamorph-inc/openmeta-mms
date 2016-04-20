/////////////////////////////////////////////////////////////////////
////                                                             ////
////  SystemC Analog GND										 ////
////                                                             ////
////  SystemC Version: 2.3.0                                     ////
////  Author: Peter Volgyesi, MetaMorph, Inc.                    ////
////          pvolgyesi@metamorphsoftware.com                    ////
////                                                             ////
////													         ////
/////////////////////////////////////////////////////////////////////

#ifndef AGND_H
#define AGND_H

#include <systemc.h>

SC_MODULE(AGND) {
	typedef sc_uint<10>	analog_type;

	sc_out<analog_type>	out;

	SC_CTOR(AGND) {
		out.initialize(0);
	}
};

#endif // AGND_H