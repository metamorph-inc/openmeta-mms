/////////////////////////////////////////////////////////////////////
////                                                             ////
////  Simple PWM as DAC Model Test Bench					     ////
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
#include <PulseOxyGlue/PulseOxyGlue.h>
#include <NJL5501R/njl5501r.h>
#include "test_PulseOxyGlue.h"

#define VCD_OUTPUT_ENABLE


int sc_main(int argc, char *argv[]) {

	sc_set_time_resolution(1.0, SC_US);

	sc_clock clk("clock", 1, SC_MS);
	sc_signal<sc_logic> ir_led_pwm, red_led_pwm, dc_pwm;
	sc_signal<sc_uint<10>>	raw_analog, filt_analog, ir_led_analog, red_led_analog, sensor_analog;

	njl5501r		i_njl5501r("NJL5501R");
	PulseOxyGlue	i_glue("GLUE");
	test_PulseOxyGlue	i_test("TEST");

	i_glue.ir_led_pwm(ir_led_pwm);
	i_glue.red_led_pwm(red_led_pwm);
	i_glue.ir_led_analog(ir_led_analog);
	i_glue.red_led_analog(red_led_analog);
	i_glue.sensor_analog(sensor_analog);
	i_glue.dc_pwm(dc_pwm);
	i_glue.raw_analog(raw_analog);
	i_glue.filt_analog(filt_analog);

	i_test.clk(clk);
	i_test.ir_led_pwm(ir_led_pwm);
	i_test.red_led_pwm(red_led_pwm);
	i_test.dc_pwm(dc_pwm);
	i_test.raw_analog(raw_analog);
	i_test.filt_analog(filt_analog);

	
	i_njl5501r.ir_led(ir_led_analog);
	i_njl5501r.red_led(red_led_analog);
	i_njl5501r.signal(sensor_analog);

#ifdef VCD_OUTPUT_ENABLE
	sc_trace_file *vcd_log = sc_create_vcd_trace_file("TEST_PulseOxyGlue");
	sc_trace(vcd_log, ir_led_pwm, "ir_led_pwm");
	sc_trace(vcd_log, ir_led_analog, "ir_led_analog");
	sc_trace(vcd_log, red_led_pwm, "red_led_pwm");
	sc_trace(vcd_log, red_led_analog, "red_led_analog");
	sc_trace(vcd_log, dc_pwm, "dc_pwm");
	sc_trace(vcd_log, sensor_analog, "sensor_analog");
	sc_trace(vcd_log, raw_analog, "raw_analog");
	sc_trace(vcd_log, filt_analog, "filt_analog");
#endif

	srand((unsigned int)(time(NULL) & 0xffffffff));
	sc_start();

#ifdef VCD_OUTPUT_ENABLE
	sc_close_vcd_trace_file(vcd_log);
#endif

	return i_test.error_cnt;
}

