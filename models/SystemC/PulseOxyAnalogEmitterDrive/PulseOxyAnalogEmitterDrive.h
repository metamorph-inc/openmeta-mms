/////////////////////////////////////////////////////////////////////
////                                                             ////
////  SystemC functional model for Pulse Oxymeter Analog Circuit ////
////                                                             ////
////  SystemC Version: 2.3.0                                     ////
////  Author: Peter Volgyesi, MetaMorph, Inc.                    ////
////          pvolgyesi@metamorphsoftware.com                    ////
////                                                             ////
////													         ////
/////////////////////////////////////////////////////////////////////

#ifndef PULSEOXYANALOGEMMITERDRIVE_H
#define PULSEOXYANALOGEMMITERDRIVE_H

#include <systemc.h>
#include <PWMDAC/pwmdac.h>

SC_MODULE(PulseOxyAnalogEmitterDrive) {

	typedef sc_uint<10>	analog_type; 
	
	// ports
	sc_in<sc_logic> ir_led_pwm;
	sc_in<sc_logic> red_led_pwm;

	sc_out<analog_type>	ir_led_analog;
	sc_out<analog_type>	red_led_analog;

	SC_CTOR(PulseOxyAnalogEmitterDrive);
	~PulseOxyAnalogEmitterDrive();

//private:
	pwmdac*	i_pwmdac_ir;
	pwmdac*	i_pwmdac_red;
};

#endif // PULSEOXYANALOGEMMITERDRIVE_H