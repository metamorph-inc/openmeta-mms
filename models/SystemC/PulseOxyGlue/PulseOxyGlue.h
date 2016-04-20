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

#ifndef PULSEOXYGLUE_H
#define PULSEOXYGLUE_H

#include <systemc.h>
#include <PulseOxyAnalogEmitterDrive/PulseOxyAnalogEmitterDrive.h>
#include <PulseOxyAnalogIn/PulseOxyAnalogIn.h>

SC_MODULE(PulseOxyGlue) {

	typedef sc_uint<10>	analog_type; 
	
	// ports
	sc_in<sc_logic> ir_led_pwm;
	sc_in<sc_logic> red_led_pwm;

	sc_out<analog_type>	ir_led_analog;
	sc_out<analog_type>	red_led_analog;
	sc_in<analog_type>	sensor_analog;

	sc_in<sc_logic> dc_pwm;
	sc_out<analog_type>	raw_analog;
	sc_out<analog_type>	filt_analog;

	SC_CTOR(PulseOxyGlue);
	~PulseOxyGlue();

//private:
	PulseOxyAnalogEmitterDrive*	i_PulseOxyAnalogEmitterDrive;
	PulseOxyAnalogIn*	i_PulseOxyAnalogIn;
};

#endif // PULSEOXYGLUE_H