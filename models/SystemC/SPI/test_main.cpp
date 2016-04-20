/////////////////////////////////////////////////////////////////////
////                                                             ////
////  Simple SPI Test Bench								     ////
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
#include <SPI/spi.h>
#include "test_spi.h"

#define VCD_OUTPUT_ENABLE

int sc_main(int argc, char *argv[]) {

	sc_set_time_resolution(1.0, SC_NS);

	sc_clock clk("clock", 20.84, SC_NS);
	sc_signal<bool>	rst;

	spi				i_spi("SPI");
	test_spi			i_test("TEST");

	i_spi.clk(clk);
	i_spi.rst(rst);

	i_test.clk(clk);
	i_test.rst(rst);

#ifdef VCD_OUTPUT_ENABLE
	sc_trace_file *vcd_log = sc_create_vcd_trace_file("TEST_SPI");
	sc_trace(vcd_log, clk, "Clk");
	sc_trace(vcd_log, rst, "Reset_n");
#endif

	srand((unsigned int)(time(NULL) & 0xffffffff));
	sc_start();

#ifdef VCD_OUTPUT_ENABLE
	sc_close_vcd_trace_file(vcd_log);
#endif

	return i_test.error_cnt;
}

