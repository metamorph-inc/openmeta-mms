/////////////////////////////////////////////////////////////////////
////                                                             ////
////  Simplified Arduino / Atmel megaAVR microcontroller model	 ////
////                                                             ////
////  SystemC Version: 2.3.0                                     ////
////  Author: Peter Volgyesi, MetaMorph, Inc.                    ////
////          pvolgyesi@metamorphsoftware.com                    ////
////                                                             ////
////													         ////
/////////////////////////////////////////////////////////////////////

#ifndef ARDUINO_H
#define ARDUINO_H

#include <systemc.h>
#include <POR/por.h>
#include <ArduinoRandom/ArduinoRandom.h>

//
// API Bridge
//
namespace ArduinoAPI {
	typedef void (*setupfn)();
	typedef void (*loopfn)();

    // Constants
    extern const int HIGH;
    extern const int LOW;

    extern const int INPUT;
    extern const int OUTPUT;
    extern const int INPUT_PULLUP;

    // Digital I/O
    extern void pinMode(int pin, int mode);
    extern void digitalWrite(int pin, int val);
    extern int digitalRead(int pin);

	extern int analogRead(int pin);
	extern void analogWrite(int pin, int val);
	extern unsigned long millis();
	extern unsigned long micros();
	extern void delay(int ms);
    extern void delayMicroseconds(unsigned int us);

	class HardwareSerial {
	public:
		void begin(int baud);
		int	read();
		int	available();
		void write(int val);
		void write(const char * text);
	};
	extern HardwareSerial Serial;

		// API methods for MOT-424
	extern long random( long max );
	extern long random( long min, long max );
	extern void randomSeed( long seed );

};


SC_MODULE(Arduino) {

public:
	// API method emulation
    void pinMode(int pin, int mode);
	void digitalWrite(int pin, int val);
	int digitalRead(int pin);
	int analogRead(int pin);
	void analogWrite(int pin, int val);
	unsigned long millis();
	unsigned long micros();
	void delay(int ms);
    void delayMicroseconds(unsigned int us);
	int	serial_read();
	int	serial_available();
	void serial_write(int val);
	void serial_write(const char * text);
	// API methods for MOT-424
	long random( long max );
	long random( long min, long max );
	void randomSeed( long seed );


	typedef sc_uint<10>	analog_type;
	typedef sc_uint<8>  app_state_type;

	const int static APP_STATE_PRESETUP=0;
	const int static APP_STATE_IDLE=1;
	const int static APP_STATE_INSETUP=4;
	const int static APP_STATE_INLOOP=3;
	const int static APP_STATE_INDELAY=2;
	const int static APP_STATE_INANALOGREAD=5;

	static const int pwm_period = 256;
	static const int analog_max = 1023;

	// Ports
    typedef enum pwm_state_ {
        PWM_ENABLED,
        PWM_DISABLED,
        PWM_NOT_AVAILABLE
    } pwm_state_t;

    typedef struct digital_io_port {
        int dir;
        int odat;
        sc_inout<sc_logic> *sig;
        pwm_state_t pwm_state;
        int pwm_duty;
    } digital_io_port_t;

    // Signals kept separate to aid code generation
    sc_inout<sc_logic> d0, d1, d2, d3, d4, d5, d6, d7, d8, d9, d10, d11, d12, d13;
    sc_in<analog_type> a0, a1, a2, a3, a4, a5;

	sc_out<app_state_type> app_state;

	static Arduino* current;
	SC_HAS_PROCESS(Arduino);
	Arduino(sc_module_name _name, ArduinoAPI::setupfn _setup, ArduinoAPI::loopfn _loop);
	~Arduino();

protected:
	sc_signal<bool> rst;
	sc_clock clk;

	sc_signal<bool> tx_data_wr;
	sc_signal<sc_uint<8>> tx_data;
	sc_signal<bool> tx_empty;

	sc_signal<bool> rx_data_rd;
	sc_signal<sc_uint<8>> rx_data;
	sc_signal<bool> rx_avail;

private:

    por*	i_por;
    ArduinoAPI::setupfn	setup;
    ArduinoAPI::loopfn	loop;

    sc_time reset_time;

    int pwm_cnt;
    digital_io_port_t d[14];
    sc_in<analog_type> *a[6];

    void main();
    void gpio_update();

    WSADATA wsaData;
    SOCKET udp_socket;
    static const int ARDUINO_GUI_PORT = 2003;
    char send_buf[8];

};

#endif // ARDUINO_H