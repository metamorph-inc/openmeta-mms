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

#include "PulseOxyAnalogIn.h"


void PulseOxyAnalogIn::filt_update()
{
	filt_analog.write(sensor_analog.read() - dc_analog.read());
	raw_analog.write(sensor_analog.read());
}


PulseOxyAnalogIn::PulseOxyAnalogIn(sc_module_name mname) : sc_module(mname)
{
	i_pwmdac_dc = new pwmdac("DC_PWMDAC");

	i_pwmdac_dc->pwm(dc_pwm);
	i_pwmdac_dc->analog(dc_analog);
	
	SC_METHOD(filt_update);
		sensitive << dc_analog << sensor_analog;
}

PulseOxyAnalogIn::~PulseOxyAnalogIn()
{
	if (i_pwmdac_dc) {
		delete i_pwmdac_dc;
	}
}
