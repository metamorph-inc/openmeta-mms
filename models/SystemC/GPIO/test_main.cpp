/////////////////////////////////////////////////////////////////////
////                                                             ////
////  Simple GPIO Test Bench								     ////
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
#include <GPIO/gpio.h>
#include "test_gpio.h"


#define VCD_OUTPUT_ENABLE


int sc_main(int argc, char *argv[]) {

	sc_set_time_resolution(1.0, SC_NS);

	sc_clock clk("clock", 20.84, SC_NS);
	sc_signal<bool>	rst;
	sc_signal<bool> data_out_wr, oe_wr;
	sc_signal<sc_uint<8>> data_out, oe, data_in;
	sc_signal_rv<8> pins;

	gpio				i_gpio("GPIO");
	test_gpio			i_test("TEST");

	i_gpio.clk(clk);
	i_gpio.rst(rst);

	i_test.clk(clk);
	i_test.rst(rst);

	i_test.data_out(data_out);
	i_test.data_out_wr(data_out_wr);
	i_test.oe(oe);
	i_test.oe_wr(oe_wr);
	i_test.data_in(data_in);
	i_test.pin(pins);

	i_gpio.data_out(data_out);
	i_gpio.data_out_wr(data_out_wr);
	i_gpio.oe(oe);
	i_gpio.oe_wr(oe_wr);
	i_gpio.data_in(data_in);
	i_gpio.pin(pins);

#ifdef VCD_OUTPUT_ENABLE
	sc_trace_file *vcd_log = sc_create_vcd_trace_file("TEST_GPIO");
	sc_trace(vcd_log, clk, "Clk");
	sc_trace(vcd_log, rst, "Reset_n");
	sc_trace(vcd_log, pins, "Pins");
#endif

	srand((unsigned int)(time(NULL) & 0xffffffff));
	sc_start();

#ifdef VCD_OUTPUT_ENABLE
	sc_close_vcd_trace_file(vcd_log);
#endif

	return i_test.error_cnt;
}

