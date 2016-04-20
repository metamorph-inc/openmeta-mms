/////////////////////////////////////////////////////////////////////
////                                                             ////
////  Simple Arduino Test Benchs                                 ////
////                                                             ////
////  SystemC Version: 2.3.0                                     ////
////  Author: Sandor Szilvasi, MetaMorph, Inc.                   ////
////          sszilvasi@metamorphsoftware.com                    ////
////                                                             ////
////                                                             ////
/////////////////////////////////////////////////////////////////////

#ifndef TEST_ARDUINO_H
#define TEST_ARDUINO_H

#include <systemc.h>
#include <Arduino/Arduino.h>

extern unsigned int test_id;
extern unsigned int test_en;

extern Arduino::analog_type analog_magic_v[6];


class test_arduino : public sc_module {
public:
    // Signals
    sc_inout<sc_logic> d0, d1, d2, d3, d4, d5, d6, d7, d8, d9, d10, d11, d12, d13;
    sc_out<Arduino::analog_type> a0, a1, a2, a3, a4, a5;

    void init(void) {
        error_cnt = 0;
        d[0] = &d0;
        d[1] = &d1;
        d[2] = &d2;
        d[3] = &d3;
        d[4] = &d4;
        d[5] = &d5;
        d[6] = &d6;
        d[7] = &d7;
        d[8] = &d8;
        d[9] = &d9;
        d[10] = &d10;
        d[11] = &d11;
        d[12] = &d12;
        d[13] = &d13;
        a[0] = &a0;
        a[1] = &a1;
        a[2] = &a2;
        a[3] = &a3;
        a[4] = &a4;
        a[5] = &a5;

        wait(200, SC_US);
        test_case_1();
        wait(100, SC_US);
        test_case_2();
        wait(100, SC_US);
        test_case_3();
        wait(100, SC_US);
        test_case_4();
        wait(100, SC_US);
        test_case_5();
		wait(100, SC_US);
		test_case_6();
        wait(200, SC_US);

        sc_stop();
    }

    SC_CTOR(test_arduino) {
       SC_THREAD(init);
    }

    int error_cnt;

private:

    sc_inout<sc_logic> *d[14];
    sc_out<Arduino::analog_type> *a[6];


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


// Arduino API calls
void test_case_1(void) {

	cout << endl;

	cout << "**************************************************" << endl;
	cout << "*** TEST CASE 1 - digitalWrite()               ***" << endl;
	cout << "**************************************************" << endl << endl;

    // Check the state change on two artibrary pins

    // #1 Expected to set D0 to '1'
    test_id = 0; 
    test_en = 1;
	wait(50, SC_US);

    if (d0.read().value() != sc_dt::Log_1) {
		cout << "ERROR: Output mismatch on D0, expected '1'" <<
					" (" << sc_time_stamp() << ")" << endl << endl;
		error_cnt++;
    }

    // #1 Expected to set D0 to '0'
    test_id = 1; 
    test_en = 1;
	wait(50, SC_US);

    if (d0.read().value() != sc_dt::Log_0) {
		cout << "ERROR: Output mismatch on D0, expected '0'" <<
					" (" << sc_time_stamp() << ")" << endl << endl;
		error_cnt++;
    }

    // #2 Expected to set D2 to '1'
    test_id = 2; 
    test_en = 1;
	wait(50, SC_US);

    if (d3.read().value() != sc_dt::Log_1) {
		cout << "ERROR: Output mismatch on D0, expected '1'" <<
					" (" << sc_time_stamp() << ")" << endl << endl;
		error_cnt++;
    }

    // #3 Expected to set D2 to '0'
    test_id = 3; 
    test_en = 1;
	wait(50, SC_US);

    if (d3.read().value() != sc_dt::Log_0) {
		cout << "ERROR: Output mismatch on D0, expected '0'" <<
					" (" << sc_time_stamp() << ")" << endl << endl;
		error_cnt++;
    }

    cout << endl;

	show_errors();

	cout << "**************************************************" << endl;
	cout << "*** TEST DONE ...                              ***" << endl;
	cout << "**************************************************" << endl << endl;
}

void test_case_2(void) {

	cout << endl;

	cout << "**************************************************" << endl;
	cout << "*** TEST CASE 2 - digitalRead()                ***" << endl;
	cout << "**************************************************" << endl << endl;
    
    // Check pin D0 for the 'result'

    test_id = 4; // Release pin 1
    test_en = 1;

	wait(50, SC_US);
    d1.write(sc_dt::Log_0);

    test_id = 5; // Loop back pin 1 value to pin 0
    test_en = 1;
	wait(50, SC_US);

    if (d0.read().value() != sc_dt::Log_0) {
		cout << "ERROR: Output mismatch on D0, expected " << sc_dt::Log_0 <<
            " but got " << d0.read() << " (" << sc_time_stamp() << ")" << endl << endl;
		error_cnt++;
    }

	cout << endl;

	show_errors();

	cout << "**************************************************" << endl;
	cout << "*** TEST DONE ...                              ***" << endl;
	cout << "**************************************************" << endl << endl;
}

void test_case_3(void) {

    cout << endl;

    cout << "**************************************************" << endl;
    cout << "*** TEST CASE 3 - analogRead()                 ***" << endl;
    cout << "**************************************************" << endl << endl;
    
    // Firmware compares read values with the stored magic reference
    // Check pin D[0-5] for the 'result'

    for (int i = 0; i < 6; i++)
    {
        a[i]->write(analog_magic_v[i]);
        d[i]->write(sc_dt::Log_Z);
    }

    wait(100, SC_US);
    test_id = 10;
    test_en = 1;
    wait(800, SC_US);

    for (int i = 0; i < 6; i++)
    {
        if (d[i]->read().value() != sc_dt::Log_1) 
        {
            cout << "ERROR: Mismatch on A" << i << ", expected 0x" << hex << analog_magic_v[i] <<
                dec << " (" << sc_time_stamp() << ")" << endl << endl;
            error_cnt++;
        }
    }

	cout << endl;

	show_errors();

	cout << "**************************************************" << endl;
	cout << "*** TEST DONE ...                              ***" << endl;
	cout << "**************************************************" << endl << endl;
}

void test_case_4(void) {

    cout << endl;

    cout << "**************************************************" << endl;
    cout << "*** TEST CASE 4 - analogWrite()                ***" << endl;
    cout << "**************************************************" << endl << endl;

    // Enable a PWM and check its duty cycle

    cout << endl;
    test_id = 20;
    test_en = 1;
    wait(50, SC_US);

    sc_time t1, t2, t3;
    sc_time to(5, SC_US); // timeout
    float meas_duty;

    wait(to, d[9]->posedge_event());
    t1 = sc_time_stamp();

    wait(to, d[9]->negedge_event());
    t2 = sc_time_stamp();

    wait(to, d[9]->posedge_event());
    t3 = sc_time_stamp();

    meas_duty = (t2-t1)/(t3-t1);
    if (meas_duty < 0.24 || meas_duty > 0.26)
    {
        cout << "ERROR: PWM duty cycle is out of range, expected 0.25, got "
            << meas_duty << " (" << sc_time_stamp() << ")" << endl << endl;
        error_cnt++;
    }

    wait(50, SC_US);

	cout << endl;

	show_errors();

	cout << "**************************************************" << endl;
	cout << "*** TEST DONE ...                              ***" << endl;
	cout << "**************************************************" << endl << endl;
}


void test_case_5(void) {

    cout << endl;

    cout << "**************************************************" << endl;
    cout << "*** TEST CASE 5 - Time                         ***" << endl;
    cout << "**************************************************" << endl << endl;
    
    // delay()
    sc_time t1, t2;
    sc_time setup(100, SC_US);
    sc_time expected(1, SC_MS);
    sc_time tolerance(2, SC_US);

    test_id = 50;
    test_en = 1;

    wait(setup, d[0]->posedge_event());
    t1 = sc_time_stamp();

    wait(2*expected, d[0]->negedge_event());
    t2 = sc_time_stamp();

    if (t2 - t1 > expected + tolerance) {
        cout << "ERROR: Observed delay of delay() is too long (expected 1 ms got "
            << (t2-t1).to_seconds()*1000 << " ms)" << endl << endl;
        error_cnt++;
    }

    // delayMicroseconds()
    expected = sc_time(50, SC_US);
    tolerance = sc_time(2, SC_US);

    test_id = 51;
    test_en = 1;

    wait(setup, d[0]->posedge_event());
    t1 = sc_time_stamp();

    wait(2*expected, d[0]->negedge_event());
    t2 = sc_time_stamp();

    if (t2 - t1 > expected + tolerance) {
        cout << "ERROR: Observed delay of delayMicroseconds() is too long (expected "
            << expected.to_seconds()*1000000 << " us got "
            << (t2-t1).to_seconds()*1000000 << " us)" << endl << endl;
        error_cnt++;
    }

    // millis()
    // NOTE: Depends on delay()

    test_id = 52;
    test_en = 1;

    wait(sc_time(5, SC_MS), d[0]->posedge_event());

    if (d[1]->read().value() != sc_dt::Log_1) {
        cout << "ERROR: millis() timing error" << endl << endl;
        error_cnt++;
    }


    // millis()
    // NOTE: Depends on delayMicroseconds()
    test_id = 53;
    test_en = 1;

    wait(sc_time(1, SC_MS), d[0]->posedge_event());

    if (d[1]->read().value() != sc_dt::Log_1) {
        cout << "ERROR: micros() timing error" << endl << endl;
        error_cnt++;
    }

    cout << endl;

	show_errors();

	cout << "**************************************************" << endl;
	cout << "*** TEST DONE ...                              ***" << endl;
	cout << "**************************************************" << endl << endl;
}


void test_case_6(void) {

    cout << endl;

    cout << "**************************************************" << endl;
    cout << "*** TEST CASE 6 - Random                       ***" << endl;
    cout << "**************************************************" << endl << endl;

    test_id = 60;
    test_en = 1;
    wait(800, SC_US);


    cout << endl;

	show_errors();

	cout << "**************************************************" << endl;
	cout << "*** TEST DONE ...                              ***" << endl;
	cout << "**************************************************" << endl << endl;
}


/////////////////////////////////////////////////////////////////////

};



#endif // TEST_ARDUINO_H