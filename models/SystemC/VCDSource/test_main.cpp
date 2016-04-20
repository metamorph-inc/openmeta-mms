/////////////////////////////////////////////////////////////////////
////                                                             ////
////  Simple VCD Source Test Bench							     ////
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
#include <VCDSource/VCDSource.h>


typedef sc_uint<10>	analog_type; 
typedef sc_int<32>	integer_type; 
typedef sc_logic	logic_type; 
typedef sc_lv<32>	logic_vector_type; 

int sc_main(int argc, char *argv[]) {

	sc_set_time_resolution(1.0, SC_NS);

	sc_signal<analog_type>  analogSignal;
	sc_signal<integer_type>  integerSignal;
	sc_signal<logic_type>  logicSignal;
	sc_signal<logic_vector_type>  logicVectorSignal;


	VCDAnalogSource			i_analogSource("VCDAnalogSource", "TEST_VCDSOURCE_IN.vcd", "vAnalog");
	VCDIntegerSource			i_integerSource("VCDIntegerSource", "TEST_VCDSOURCE_IN.vcd", "vInteger");
	VCDLogicSource			i_logicSource("VCDLogicSource", "TEST_VCDSOURCE_IN.vcd", "vLogic");
	VCDLogicVectorSource			i_logicVectorSource("VCDLogicVectorSource", "TEST_VCDSOURCE_IN.vcd", "vLogicVector");
	
	i_analogSource.out(analogSignal);
	i_integerSource(integerSignal);
	i_logicSource(logicSignal);
	i_logicVectorSource(logicVectorSignal);

	sc_trace_file *vcd_log = sc_create_vcd_trace_file("TEST_VCDSOURCE");
	sc_trace(vcd_log, analogSignal, "analogSignal");
	sc_trace(vcd_log, integerSignal, "integerSignal");
	sc_trace(vcd_log, logicSignal, "logicSignal");
	sc_trace(vcd_log, logicVectorSignal, "logicVectorSignal");

	srand((unsigned int)(time(NULL) & 0xffffffff));
	sc_start();

	sc_close_vcd_trace_file(vcd_log);
	
//getchar();
	return 0;
}
