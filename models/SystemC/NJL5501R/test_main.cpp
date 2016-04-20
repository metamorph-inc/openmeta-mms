/////////////////////////////////////////////////////////////////////
////                                                             ////
////  Simple NJL5501R (+patient) Test Bench					     ////
////                                                             ////
////  SystemC Version: 2.3.0                                     ////
////  Author: Peter Volgyesi, MetaMorph, Inc.                    ////
////          pvolgyesi@metamorphsoftware.com                    ////
////                                                             ////
////                                                             ////
/////////////////////////////////////////////////////////////////////
#include <stdlib.h>
#include <time.h>
#include <systemc.h>
#include <NJL5501R/njl5501r.h>
#include "test_njl5501r.h"

#define VCD_OUTPUT_ENABLE

int sc_main(int argc, char *argv[]) {

	sc_set_time_resolution(1.0, SC_US);

	sc_clock clk("clock", 1, SC_MS);
	sc_signal<sc_uint<10>>	signal, red_led, ir_led;

	njl5501r		i_njl5501r("NJL5501R");
	test_njl5501r			i_test("TEST");

	i_njl5501r.red_led(red_led);
	i_njl5501r.ir_led(ir_led);
	i_njl5501r.signal(signal);

	i_test.clk(clk);
	i_test.red_led(red_led);
	i_test.ir_led(ir_led);

#ifdef VCD_OUTPUT_ENABLE
	sc_trace_file *vcd_log = sc_create_vcd_trace_file("TEST_NJL5501R");
	sc_trace(vcd_log, red_led, "Red_LED");
	sc_trace(vcd_log, ir_led, "IR_LED");

	sc_trace(vcd_log, signal, "Signal");
#endif

	srand((unsigned int)(time(NULL) & 0xffffffff));
	sc_start();

#ifdef VCD_OUTPUT_ENABLE
	sc_close_vcd_trace_file(vcd_log);
#endif

	return i_test.error_cnt;
}

