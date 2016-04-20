/////////////////////////////////////////////////////////////////////
////                                                             ////
////  Simple Arduino Test Bench                                  ////
////                                                             ////
////  SystemC Version: 2.3.0                                     ////
////  Author: Sandor Szilvasi, MetaMorph, Inc.                   ////
////          sszilvasi@metamorphsoftware.com                    ////
////                                                             ////
////                                                             ////
/////////////////////////////////////////////////////////////////////

#include <stdlib.h>
#include <time.h>
#include <systemc.h>
#include <Arduino/Arduino.h>
#include "test_Arduino.h"

#define VCD_OUTPUT_ENABLE

unsigned int test_id = 0;
unsigned int test_en = 1;

Arduino::analog_type analog_magic_v[6] = {0x011, 0x122, 0x033, 0x144, 0x055, 0x166};

namespace ArduinoFirmware {
using namespace ArduinoAPI;

#include "./test_ArduinoAPI.ino"

} // namespace firmware


int sc_main(int argc, char *argv[]) {

	std::cout << "sc_main() invoked" << std::endl;
	sc_set_time_resolution(1.0, SC_NS);

	// Signals
	typedef sc_uint<10>	analog_type;
	typedef sc_uint<8>  app_state_type;

    sc_signal_resolved d0, d1, d2, d3, d4, d5, d6, d7, d8, d9, d10, d11, d12, d13;
    sc_signal<Arduino::analog_type> a0, a1, a2, a3, a4, a5;

	sc_signal<Arduino::app_state_type>	app_state;

	// Comopnents
	Arduino i_uut("Arduino", ArduinoFirmware::setup, ArduinoFirmware::loop);
	test_arduino i_tb("TEST");

	// Port maps
	i_uut.d0(d0);
	i_uut.d1(d1);
	i_uut.d2(d2);
	i_uut.d3(d3);
	i_uut.d4(d4);
	i_uut.d5(d5);
	i_uut.d6(d6);
	i_uut.d7(d7);
	i_uut.d8(d8);
	i_uut.d9(d9);
	i_uut.d10(d10);
    i_uut.d11(d11);
	i_uut.d12(d12);
	i_uut.d13(d13);
	i_uut.a0(a0);
	i_uut.a1(a1);
	i_uut.a2(a2);
	i_uut.a3(a3);
	i_uut.a4(a4);
	i_uut.a5(a5);
	i_uut.app_state(app_state);

    i_tb.d0(d0);
    i_tb.d1(d1);
    i_tb.d2(d2);
    i_tb.d3(d3);
    i_tb.d4(d4);
    i_tb.d5(d5);
    i_tb.d6(d6);
    i_tb.d7(d7);
    i_tb.d8(d8);
    i_tb.d9(d9);
    i_tb.d10(d10);
    i_tb.d11(d11);
    i_tb.d12(d12);
    i_tb.d13(d13);
    i_tb.a0(a0);
    i_tb.a1(a1);
    i_tb.a2(a2);
    i_tb.a3(a3);
    i_tb.a4(a4);
    i_tb.a5(a5);

#ifdef VCD_OUTPUT_ENABLE
	sc_trace_file *vcd_log = sc_create_vcd_trace_file("TEST_Arduino");
	sc_trace(vcd_log, d0, "d0");
	sc_trace(vcd_log, d1, "d1");
	sc_trace(vcd_log, d2, "d2");
	sc_trace(vcd_log, d3, "d3");
	sc_trace(vcd_log, d4, "d4");
	sc_trace(vcd_log, d5, "d5");
	sc_trace(vcd_log, d6, "d6");
	sc_trace(vcd_log, d7, "d7");
	sc_trace(vcd_log, d8, "d8");
	sc_trace(vcd_log, d9, "d9");
	sc_trace(vcd_log, d10, "d10");
	sc_trace(vcd_log, d11, "d11");
	sc_trace(vcd_log, d12, "d12");
	sc_trace(vcd_log, d13, "d13");
	sc_trace(vcd_log, a0, "a0");
	sc_trace(vcd_log, a1, "a1");
	sc_trace(vcd_log, a1, "a2");
	sc_trace(vcd_log, a1, "a3");
	sc_trace(vcd_log, app_state, "app_state");
#endif

	srand((unsigned int)(time(NULL) & 0xffffffff));
	sc_start();

#ifdef VCD_OUTPUT_ENABLE
	sc_close_vcd_trace_file(vcd_log);
#endif
	
	return i_tb.error_cnt;
}

