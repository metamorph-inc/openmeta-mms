/////////////////////////////////////////////////////////////////////
////                                                             ////
////  Simple CCLED Test Bench                                    ////
////                                                             ////
////  SystemC Version: 2.3.0                                     ////
////                                                             ////
////                                                             ////
/////////////////////////////////////////////////////////////////////

//#include <stdlib.h>
//#include <time.h>
//#include <systemc.h>
#include <CCLED/ccled.h>
#include "test_ccled.h"

#define VCD_OUTPUT_ENABLE

int sc_main(int argc, char *argv[]) {

	sc_set_time_resolution(1.0, SC_NS);
	sc_clock test_clk("test_clk", 1, SC_US);

	sc_signal<sc_logic> sdi_clk;
	sc_signal<sc_logic>	sdi;
	sc_signal<sc_logic>	sdo;
	sc_signal<sc_logic> le;
	sc_signal<sc_logic> oeBar;
	sc_signal<sc_logic> out0;
	sc_signal<sc_logic> out1;
	sc_signal<sc_logic> out2;
	sc_signal<sc_logic> out3;
	sc_signal<sc_logic> out4;
	sc_signal<sc_logic> out5;
	sc_signal<sc_logic> out6;
	sc_signal<sc_logic> out7;

	ccled			i_ccled("I_CCLED");
	test_ccled		i_test("TEST");

	i_ccled.sdi_clk(sdi_clk);
	i_ccled.sdi(sdi);
	i_ccled.sdo(sdo);
	i_ccled.le(le);
	i_ccled.oeBar(oeBar);
	i_ccled.out0(out0);
	i_ccled.out1(out1);
	i_ccled.out2(out2);
	i_ccled.out3(out3);
	i_ccled.out4(out4);
	i_ccled.out5(out5);
	i_ccled.out6(out6);
	i_ccled.out7(out7);

	
	i_test.test_clk(test_clk);
	i_test.sdi_clk(sdi_clk);
	i_test.sdi(sdi);
	i_test.sdo(sdo);
	i_test.le(le);
	i_test.oeBar(oeBar);
	i_test.out0(out0);
	i_test.out1(out1);
	i_test.out2(out2);
	i_test.out3(out3);
	i_test.out4(out4);
	i_test.out5(out5);
	i_test.out6(out6);
	i_test.out7(out7);

#ifdef VCD_OUTPUT_ENABLE
	sc_trace_file *vcd_log = sc_create_vcd_trace_file("TEST_CCLED");
	sc_trace(vcd_log, sdi_clk,		"CLK");
	sc_trace(vcd_log, sdi,		"SDI");
	sc_trace(vcd_log, sdo,		"SDO");
	sc_trace(vcd_log, le,		"LE");
	sc_trace(vcd_log, oeBar,	"/OE");

	sc_trace(vcd_log, out0, "OUT0");
	sc_trace(vcd_log, out1, "OUT1");
	sc_trace(vcd_log, out2, "OUT2");
	sc_trace(vcd_log, out3, "OUT3");
	sc_trace(vcd_log, out4, "OUT4");
	sc_trace(vcd_log, out5, "OUT5");
	sc_trace(vcd_log, out6, "OUT6");
	sc_trace(vcd_log, out7, "OUT7");
#endif

	// Start the simulation.
	sc_start();

#ifdef VCD_OUTPUT_ENABLE
	sc_close_vcd_trace_file(vcd_log);
#endif

	return i_test.error_cnt;
}

