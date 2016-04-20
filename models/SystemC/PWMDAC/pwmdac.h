/////////////////////////////////////////////////////////////////////
////                                                             ////
////  Simple PWM as DAC Model	                			     ////
////                                                             ////
////  SystemC Version: 2.3.0                                     ////
////  Author: Peter Volgyesi, MetaMorph, Inc.                    ////
////          pvolgyesi@metamorphsoftware.com                    ////
////                                                             ////
////													         ////
/////////////////////////////////////////////////////////////////////

#ifndef PWMDAC_H
#define PWMDAC_H

#include <systemc.h>

SC_MODULE(pwmdac) {

	typedef sc_uint<10>	analog_type; 
	// ports
	sc_in<sc_logic> pwm;
	sc_out<analog_type>	analog;

	SC_CTOR(pwmdac);

//private:
	double		full_scale;
	sc_time		pos_time;
	sc_time		neg_time;

	void analog_update();
};

#endif // PWMDAC_H