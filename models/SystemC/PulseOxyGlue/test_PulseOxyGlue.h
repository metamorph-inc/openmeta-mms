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

#ifndef TEST_PULSEOXYGLUE_H
#define TEST_PULSEOXYGLUE_H

#include <systemc.h>
#include <PulseOxyGlue/PulseOxyGlue.h>

SC_MODULE(test_PulseOxyGlue) {

	sc_in<bool>    clk;

	// DUT ports
	sc_out<sc_logic>    ir_led_pwm;
	sc_out<sc_logic>    red_led_pwm;
	sc_out<sc_logic>    dc_pwm;

	sc_in<sc_uint<10>>	raw_analog;
	sc_in<sc_uint<10>>	filt_analog;

	// Signals

	// Local Vars
	int error_cnt;

	static const int pwm_period = 256;
	int ir_led_pwm_duty;
	int red_led_pwm_duty;
	int dc_pwm_duty;

/////////////////////////////////////////////////////////////////////
////                                                             ////
////              Test Bench Library                             ////
////                                                             ////
/////////////////////////////////////////////////////////////////////

void show_errors(void) {
	cout << "+----------------------+" << endl;
	cout << "| TOTAL ERRORS: " << error_cnt << endl;
	cout << "+----------------------+" << endl << endl;
}



/////////////////////////////////////////////////////////////////////

/////////////////////////////////////////////////////////////////////
////                                                             ////
////              Test Case Collection                           ////
////                                                             ////
/////////////////////////////////////////////////////////////////////

void test_case_1(void) {
	cout << endl;

	cout << "**************************************************" << endl;
	cout << "*** TEST CASE 1                                ***" << endl;
	cout << "**************************************************" << endl << endl;

	ir_led_pwm_duty = 170;
	red_led_pwm_duty = 1;
	dc_pwm_duty = 1;
	for (int i = 0; i < 10000; i++) wait(clk.posedge_event());
	dc_pwm_duty = 50;
	for (int i = 0; i < 10000; i++) wait(clk.posedge_event());
	ir_led_pwm_duty = 1;
	red_led_pwm_duty = 240;
	for (int i = 0; i < 10000; i++) wait(clk.posedge_event());

	cout << endl;
	show_errors();

	cout << "**************************************************" << endl;
	cout << "*** TEST DONE ...                              ***" << endl;
	cout << "**************************************************" << endl << endl;
}

/////////////////////////////////////////////////////////////////////

	
	void init(void) {
		error_cnt = 0;

		//for (int i = 0; i < 10; i++) wait(clk.posedge_event());

		test_case_1();

		//for (int i = 0; i < 10; i++) wait(clk.posedge_event());
		sc_stop();
	}

	SC_CTOR(test_PulseOxyGlue) {
		ir_led_pwm_duty = 0;
		red_led_pwm_duty = 0;
		dc_pwm_duty = 0;
	
		SC_THREAD(init);
			sensitive << clk.pos();
		SC_THREAD(ir_led_pwm_gen);
			sensitive << clk.pos();
		SC_THREAD(red_led_pwm_gen);
			sensitive << clk.pos();
		SC_THREAD(dc_pwm_gen);
			sensitive << clk.pos();
	}

	void ir_led_pwm_gen() {
		while (true) {
			int c = 0;
            ir_led_pwm.write(sc_dt::Log_1);
			for (; c < ir_led_pwm_duty; c++) wait(clk.posedge_event());
            ir_led_pwm.write(sc_dt::Log_0);
			for (; c < pwm_period; c++) wait(clk.posedge_event());
		}
	}

	void red_led_pwm_gen() {
		while (true) {
			int c = 0;
            red_led_pwm.write(sc_dt::Log_1);
			for (; c < red_led_pwm_duty; c++) wait(clk.posedge_event());
			red_led_pwm.write(sc_dt::Log_0);
			for (; c < pwm_period; c++) wait(clk.posedge_event());
		}
	}

	void dc_pwm_gen() {
		while (true) {
			int c = 0;
			dc_pwm.write(sc_dt::Log_1);
			for (; c < dc_pwm_duty; c++) wait(clk.posedge_event());
			dc_pwm.write(sc_dt::Log_0);
			for (; c < pwm_period; c++) wait(clk.posedge_event());
		}
	}
};


#endif // TEST_PULSEOXYGLUE_H
