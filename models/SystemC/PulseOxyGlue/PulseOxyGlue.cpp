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

#include "PulseOxyGlue.h"


PulseOxyGlue::PulseOxyGlue(sc_module_name mname) : sc_module(mname)
{
	i_PulseOxyAnalogEmitterDrive = new PulseOxyAnalogEmitterDrive("ANALOG_EMITTER_DRIVE");
	i_PulseOxyAnalogIn = new PulseOxyAnalogIn("ANALOG_IN");

	i_PulseOxyAnalogEmitterDrive->ir_led_pwm(ir_led_pwm);
	i_PulseOxyAnalogEmitterDrive->red_led_pwm(red_led_pwm);
	i_PulseOxyAnalogEmitterDrive->ir_led_analog(ir_led_analog);
	i_PulseOxyAnalogEmitterDrive->red_led_analog(red_led_analog);

	i_PulseOxyAnalogIn->sensor_analog(sensor_analog);
	i_PulseOxyAnalogIn->dc_pwm(dc_pwm);
	i_PulseOxyAnalogIn->raw_analog(raw_analog);
	i_PulseOxyAnalogIn->filt_analog(filt_analog);
}

PulseOxyGlue::~PulseOxyGlue()
{
	if (i_PulseOxyAnalogEmitterDrive) {
		delete i_PulseOxyAnalogEmitterDrive;
	}
	if (i_PulseOxyAnalogIn) {
		delete i_PulseOxyAnalogIn;
	}
}
