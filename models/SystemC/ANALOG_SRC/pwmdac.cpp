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

#include "pwmdac.h"

void pwmdac::analog_update()
{
	sc_time now = sc_time_stamp();
	double period = 0, duty = 0;
	switch (pwm.read().value())
    {
    case sc_dt::Log_1:
		period = now.to_double() - pos_time.to_double();
		duty = neg_time.to_double() - pos_time.to_double();
		pos_time = now;
        break;
    case sc_dt::Log_0:
		period = now.to_double() - neg_time.to_double();
		duty = now.to_double() - pos_time.to_double();
		neg_time = now;
        break;
    default:
        cout << "WARNING: PWMDAC analog_update() invalid input value" << endl;
    }
	if (duty > 0) {
		analog.write(duty / period * full_scale);
	}
}


pwmdac::pwmdac(sc_module_name mname) : sc_module(mname),
	pos_time(), neg_time() 
{
	analog_type	tmp;

	full_scale = ((1 << tmp.length()) - 1);
	SC_METHOD(analog_update);
		sensitive << pwm;
}