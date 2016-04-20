/////////////////////////////////////////////////////////////////////
////                                                             ////
////  Simple CCLED Test Bench                                    ////
////                                                             ////
////  SystemC Version: 2.3.0                                     ////
////                                                             ////
////                                                             ////
/////////////////////////////////////////////////////////////////////


#ifndef TEST_CCLED_H
#define TEST_CCLED_H


#include <systemc.h>
#include <CCLED/ccled.h>


// Test module for checking the CCLED component:
SC_MODULE(test_ccled) {

	sc_in<bool>   test_clk;	// Common shift register clock

	// Outputs to drive the CCLED:
	sc_out<sc_logic>  sdi_clk;
	sc_out<sc_logic>  sdi;
	sc_out<sc_logic>  le;
	sc_out<sc_logic>  oeBar;

	// Inputs from the CCLED:
	sc_in<sc_logic>	 sdo;
	sc_in<sc_logic> out0;
	sc_in<sc_logic> out1;
	sc_in<sc_logic> out2;
	sc_in<sc_logic> out3;
	sc_in<sc_logic> out4;
	sc_in<sc_logic> out5;
	sc_in<sc_logic> out6;
	sc_in<sc_logic> out7;

	// Local Vars
	int error_cnt;

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

int readOutputs()
{
	int tmp = 0;
	const sc_logic low = SC_LOGIC_0;

	if( low != out7 )
	{
		tmp |= 1;
	}
	tmp <<= 1;

	if( low != out6 )
	{
		tmp |= 1;
	}
	tmp <<= 1;

	if( low != out5 )
	{
		tmp |= 1;
	}
	tmp <<= 1;

	if( low != out4 )
	{
		tmp |= 1;
	}
	tmp <<= 1;

	if( low != out3 )
	{
		tmp |= 1;
	}
	tmp <<= 1;

	if( low != out2 )
	{
		tmp |= 1;
	}
	tmp <<= 1;

	if( low != out1 )
	{
		tmp |= 1;
	}
	tmp <<= 1;

	if( low != out0 )
	{
		tmp |= 1;
	}

	return tmp;
}


void test_case_1(void) {

	cout << endl;

	cout << "**************************************************" << endl;
	cout << "*** TEST CASE 1                                ***" << endl;
	cout << "**************************************************" << endl << endl;

	cout << endl;

	const int ledPattern1 = 0x3A;

	const int hi_z = 0xff;
	const int outLedPattern = ledPattern1;

	// Set the initial conditions
	sdi_clk.write( SC_LOGIC_0 );
	le.write( SC_LOGIC_0 );
	oeBar.write( SC_LOGIC_1 );

	wait(test_clk.posedge_event());

	// Check that the output is high impedance when oeBar is high.
	int tmp = readOutputs();
	if (hi_z != tmp) {
		cout << "ERROR: Output register is not high impedance: " << tmp <<
					" (" << sc_time_stamp() << ")" << endl << endl;
		error_cnt++;
	}

	// Shift data bits into the shift register
	int input = ledPattern1;
	for( int bitCount = 0; bitCount < REGISTER_SZ; bitCount++ )
	{
		int newBit = (input >> (MAX_REGISTER_BIT_NO - bitCount)) & 1;
		sdi.write( sc_logic( newBit ) );
		wait(test_clk.posedge_event());
		sdi_clk.write( SC_LOGIC_1 );
		wait(test_clk.posedge_event());
		sdi_clk.write( SC_LOGIC_0 );
	}

	// Check that the SDO bit shows the expected bit value.
	sc_logic expectedSdo = sc_logic((ledPattern1 >> MAX_REGISTER_BIT_NO) & 1);
	sc_logic sdoValue = sdo.read();
	if( sdoValue != expectedSdo )
	{
		cout << "ERROR: SDO value was " << sdoValue << "; expected " << expectedSdo <<
					". (" << sc_time_stamp() << ")" << endl << endl;
		error_cnt++;

	}

	// Latch the data and enable the output
	le.write( SC_LOGIC_1 );
	oeBar.write( SC_LOGIC_0 );
	wait(test_clk.posedge_event());

	// Verify the the output is correct.
	tmp = readOutputs();
	const int expected = (~outLedPattern & 0xff);
	if( tmp != expected )
	{
				cout << "ERROR: output value was " << tmp << "; expected " << expected <<
					". (" << sc_time_stamp() << ")" << endl << endl;
		error_cnt++;
	}

	// Don't latch new data.
	le.write( SC_LOGIC_0 );
	wait(test_clk.posedge_event());

	// Shift ones into the shift register
	input = ledPattern1;
	for( int bitCount = 0; bitCount < REGISTER_SZ; bitCount++ )
	{
		sdi.write( SC_LOGIC_1 );
		wait(test_clk.posedge_event());
		sdi_clk.write( SC_LOGIC_1 );
		wait(test_clk.posedge_event());
		sdi_clk.write( SC_LOGIC_0 );
	}

	// Verify that SDO shows a one.
	expectedSdo = SC_LOGIC_1;
	sdoValue = sdo.read();
	if( sdoValue != expectedSdo )
	{
		cout << "ERROR: SDO value was " << sdoValue << "; expected " << expectedSdo <<
					". (" << sc_time_stamp() << ")" << endl << endl;
		error_cnt++;
	}

	// Verify the the output is still correct.
	tmp = readOutputs();
	if( tmp != expected )
	{
				cout << "ERROR: latched output value was " << tmp << "; expected " << expected <<
					". (" << sc_time_stamp() << ")" << endl << endl;
		error_cnt++;
	}

	// Disable the output
	oeBar.write( SC_LOGIC_1 );
	wait(test_clk.posedge_event());

	// Check that the output is high impedance when oeBar is high.
	tmp = readOutputs();
	if (hi_z != tmp) {
		cout << "ERROR: Final output register is not high impedance: " << tmp <<
					" (" << sc_time_stamp() << ")" << endl << endl;
		error_cnt++;
	}

	show_errors();

	cout << "**************************************************" << endl;
	cout << "*** TEST DONE ...                              ***" << endl;
	cout << "**************************************************" << endl << endl;
}

/////////////////////////////////////////////////////////////////////

	void init(void) {
		error_cnt = 0;

		test_case_1();

		for (int i = 0; i < 5; i++) wait(test_clk.posedge_event());
		sc_stop();
	}

	SC_CTOR(test_ccled) {
		SC_THREAD(init);
		sensitive << test_clk.pos();
	}
};


#endif // TEST_CCLED_H