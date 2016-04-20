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

#include "PulseOxyAnalogEmitterDrive.h"


PulseOxyAnalogEmitterDrive::PulseOxyAnalogEmitterDrive(sc_module_name mname) : sc_module(mname)
{
	i_pwmdac_ir = new pwmdac("IR_PWMDAC");
	i_pwmdac_red = new pwmdac("RED_PWMDAC");

	i_pwmdac_ir->pwm(ir_led_pwm);
	i_pwmdac_ir->analog(ir_led_analog);
	i_pwmdac_red->pwm(red_led_pwm);
	i_pwmdac_red->analog(red_led_analog);
}

PulseOxyAnalogEmitterDrive::~PulseOxyAnalogEmitterDrive()
{
	if (i_pwmdac_ir) {
		delete i_pwmdac_ir;
	}
	if (i_pwmdac_red) {
		delete i_pwmdac_red;
	}
}
