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

#ifndef PULSEOXYANALOGIN_H
#define PULSEOXYANALOGIN_H

#include <systemc.h>
#include <PWMDAC/pwmdac.h>

SC_MODULE(PulseOxyAnalogIn) {

	typedef sc_uint<10>	analog_type; 
	
	// ports
	sc_in<analog_type>	sensor_analog;

	sc_in<sc_logic>     dc_pwm;
	sc_out<analog_type> raw_analog;
	sc_out<analog_type> filt_analog;

	SC_CTOR(PulseOxyAnalogIn);
	~PulseOxyAnalogIn();

//private:
	sc_signal<analog_type>	dc_analog;
	pwmdac*	i_pwmdac_dc;

	void filt_update();
};

#endif // PULSEOXYANALOGIN_H