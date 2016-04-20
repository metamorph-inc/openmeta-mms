/////////////////////////////////////////////////////////////////////
////                                                             ////
////  Android Application for SCBus Testing (uC modul + software)////
////                                                             ////
////  SystemC Version: 2.3.0                                     ////
////  Author: Peter Volgyesi, MetaMorph, Inc.                    ////
////          pvolgyesi@metamorphsoftware.com                    ////
////                                                             ////
////                                                             ////
/////////////////////////////////////////////////////////////////////

#include <stdlib.h>
#include <time.h>
#include <SCBusApp/SCBusApp.h>
#include <POR/por.h>
#include "test_SCBusApp.h"

#define VCD_OUTPUT_ENABLE

int sc_main(int argc, char *argv[]) {

	sc_set_time_resolution(1.0, SC_US);

	sc_signal<bool> clk;
	sc_signal<bool> rst;
	sc_signal<sc_logic>	pwm0, pwm1, pwm2;
	sc_signal<sc_uint<10>>	analog0, analog1;
	sc_signal<ArduinoSCBus::app_state_type> app_state;

	SCBusApp	  i_app("APP");
	test_SCBusApp i_test("TEST");

	i_app.pwm0(pwm0);
	i_app.pwm1(pwm1);
	i_app.pwm2(pwm2);
	i_app.analog0(analog0);
	i_app.analog1(analog1);
	i_app.app_state(app_state);

	i_test.analog0(analog0);
	i_test.analog1(analog1);

#ifdef VCD_OUTPUT_ENABLE
	sc_trace_file *vcd_log = sc_create_vcd_trace_file("TEST_SCBusApp");
	sc_trace(vcd_log, pwm0, "pwm0");
	sc_trace(vcd_log, pwm1, "pwm1");
	sc_trace(vcd_log, pwm2, "pwm2");
	sc_trace(vcd_log, analog0, "analog0");
	sc_trace(vcd_log, analog1, "analog1");
	sc_trace(vcd_log, app_state, "app_state");
#endif

	srand((unsigned int)(time(NULL) & 0xffffffff));
	sc_start();

#ifdef VCD_OUTPUT_ENABLE
	sc_close_vcd_trace_file(vcd_log);
#endif

	return 0;
}

