/////////////////////////////////////////////////////////////////////
////                                                             ////
////  Pulse Oxymeter Application (uC modul + software) testbench ////
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
#include <POR/por.h>
#include <UART/uart.h>
#include <NJL5501R/njl5501r.h>
#include <PulseOxyGlue/PulseOxyGlue.h>
#include <PulseOxyApp/PulseOxyApp.h>
#include <FT232R/ft232r.h>
#include "test_PulseOxyApp_USB.h"

#define VCD_OUTPUT_ENABLE

int sc_main(int argc, char *argv[]) {

	sc_set_time_resolution(1.0, SC_NS);

	sc_signal<bool> clk;
	sc_signal<bool> rst;
	sc_signal<bool> txd, rxd;
	sc_signal<sc_logic> ir_led_pwm, red_led_pwm, dc_pwm;
	sc_signal<sc_uint<10>>	raw_analog, filt_analog, ir_led_analog, red_led_analog, sensor_analog;
	sc_signal<Arduino::app_state_type> app_state;

	sc_signal<bool> usb_tx_dp, usb_tx_dn, usb_tx_oe_nc, usb_tx_oe2_nc, usb_rx_dp, usb_rx_dn;

	njl5501r		i_njl5501r("NJL5501R");
	PulseOxyGlue	i_glue("GLUE");
	PulseOxyApp		i_app("APP");
	ft232r			i_ft232r("FT232");
	test_PulseOxyApp_USB i_test("TEST");

	i_app.txd(txd);
	i_app.rxd(rxd);
	i_app.ir_led_pwm(ir_led_pwm);
	i_app.red_led_pwm(red_led_pwm);
	i_app.dc_pwm(dc_pwm);
	i_app.raw_analog(raw_analog);
	i_app.filt_analog(filt_analog);
	i_app.app_state(app_state);

	i_ft232r.uart_txd(rxd);
	i_ft232r.uart_rxd(txd);

	i_ft232r.tx_dp(usb_tx_dp);
	i_ft232r.tx_dn(usb_tx_dn);
	i_ft232r.tx_oe(usb_tx_oe_nc);
	i_ft232r.rx_dp(usb_rx_dp);
	i_ft232r.rx_dn(usb_rx_dn);
	i_ft232r.rx_d(usb_rx_dp);

	i_test.tx_dp(usb_rx_dp);
	i_test.tx_dn(usb_rx_dn);
	i_test.tx_oe(usb_tx_oe2_nc);
	i_test.rx_dp(usb_tx_dp);
	i_test.rx_dn(usb_tx_dn);
	i_test.rx_d(usb_tx_dp);

	i_glue.ir_led_pwm(ir_led_pwm);
	i_glue.red_led_pwm(red_led_pwm);
	i_glue.ir_led_analog(ir_led_analog);
	i_glue.red_led_analog(red_led_analog);
	i_glue.sensor_analog(sensor_analog);
	i_glue.dc_pwm(dc_pwm);
	i_glue.raw_analog(raw_analog);
	i_glue.filt_analog(filt_analog);

	i_njl5501r.ir_led(ir_led_analog);
	i_njl5501r.red_led(red_led_analog);
	i_njl5501r.signal(sensor_analog);

#ifdef VCD_OUTPUT_ENABLE
	sc_trace_file *vcd_log = sc_create_vcd_trace_file("TEST_PulseOxyApp_USB");
	
	sc_trace(vcd_log, usb_tx_dp, "usb_tx_dp");
	sc_trace(vcd_log, usb_rx_dp, "usb_rx_dp");

	sc_trace(vcd_log, txd, "TXD");
	sc_trace(vcd_log, rxd, "RXD");
	sc_trace(vcd_log, ir_led_pwm, "ir_led_pwm");
	sc_trace(vcd_log, ir_led_analog, "ir_led_analog");
	sc_trace(vcd_log, red_led_pwm, "red_led_pwm");
	sc_trace(vcd_log, red_led_analog, "red_led_analog");
	sc_trace(vcd_log, dc_pwm, "dc_pwm");
	sc_trace(vcd_log, sensor_analog, "sensor_analog");
	sc_trace(vcd_log, raw_analog, "raw_analog");
	sc_trace(vcd_log, filt_analog, "filt_analog");
	sc_trace(vcd_log, app_state, "app_state");
#endif

	srand((unsigned int)(time(NULL) & 0xffffffff));
	sc_start();

#ifdef VCD_OUTPUT_ENABLE
	sc_close_vcd_trace_file(vcd_log);
#endif

	return i_test.error_cnt;
}

