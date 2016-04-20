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

#include "Arduino.h"
#pragma comment(lib,"ws2_32.lib")

Arduino* Arduino::current = NULL;
#define TICK do {wait(clk.posedge_event());} while (false)

static void print_socket_error(const char* prefix) {
    LPTSTR  errMsg;
    FormatMessage( FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM,
            NULL,
            WSAGetLastError(),
            0,
            (LPTSTR)&errMsg,
            0,
            NULL);
    cerr << prefix << errMsg << endl;
    LocalFree(errMsg);
}

namespace ArduinoAPI {

    const int HIGH = 0x1;
    const int LOW = 0x0;

    const int INPUT = 0x0;
    const int OUTPUT = 0x1;
    const int INPUT_PULLUP = 0x2;

    void pinMode(int pin, int mode)
    {
        Arduino::current->pinMode(pin, mode);
    }

    void digitalWrite(int pin, int val)
    {
        Arduino::current->digitalWrite(pin, val);
    }

    int digitalRead(int pin)
    {
        return Arduino::current->digitalRead(pin);
    }

	int analogRead(int pin) 
	{
		return Arduino::current->analogRead(pin);
	}

	void analogWrite(int pin, int val)
	{
		Arduino::current->analogWrite(pin, val);
	}

    unsigned long millis()
    {
        return Arduino::current->millis();
    }

	unsigned long micros()
    {
        return Arduino::current->micros();
    }

	void delay(int ms)
	{
		Arduino::current->delay(ms);
	}

    void delayMicroseconds(unsigned int us)
    {
        Arduino::current->delayMicroseconds(us);
    }
	
	void HardwareSerial::begin(int baud) {
		// ignoring this for now
	}
	
	int	HardwareSerial::read() {
		return Arduino::current->serial_read();
	}

	int	HardwareSerial::available() {
		return Arduino::current->serial_available();
	}

	void HardwareSerial::write(int val) {
		Arduino::current->serial_write(val);
	}

	void HardwareSerial::write(const char * text) {
		if( NULL != text )
		{
			for ( int c = *text; 0 != c; c = *++text )
			{
				Arduino::current->serial_write(c);
			}
		}
	}

	HardwareSerial Serial;

	// API methods for MOT-424
	long random( long max ) {
		return Arduino::current->random(max);
	}

	long random( long min, long max ){
		return Arduino::current->random(min, max);
	}

	void randomSeed( long seed ){
		Arduino::current->randomSeed(seed);
	}

}

long Arduino::random( long max )
{
	cout << "Arduino [" << name() << "]: random(" << max << ") @ " << sc_time_stamp() << endl;
	return ArduinoRandom::random( 0, max );
}

long Arduino::random( long min, long max )
{
	cout << "Arduino [" << name() << "]: random(" << min << ", " << max << ") @ " << sc_time_stamp() << endl;
	return ArduinoRandom::random(min, max);
}

void Arduino::randomSeed( long seed )
{
	cout << "Arduino [" << name() << "]: randomSeed(" << seed << ") @ " << sc_time_stamp() << endl;
	ArduinoRandom::randomSeed(seed);
}

void Arduino::pinMode(int pin, int mode)
{
	cout << "Arduino [" << name() << "]: pinMode() @ " << sc_time_stamp() << endl;
	TICK;

    if (pin > 13)
    {
        cout << "WARNING: Arduino pinMode() unsupported pin: " << pin << endl;
        return;
    }

    if (mode == ArduinoAPI::INPUT_PULLUP)
    {
        cout << "WARNING: Arduino pinMode() for INPUT_PULLUP is not supported" << endl;
        return;
    }

    d[pin].dir = mode;

    Arduino::current = this;
}

void Arduino::digitalWrite(int pin, int val)
{
	cout << "Arduino [" << name() << "]: digitalWrite() @ " << sc_time_stamp() << endl;
	TICK;

    if (pin > 13)
    {
        cout << "WARNING: Arduino digitalWrite() unsupported pin: " << pin << endl;
        return;
    }

    if (d[pin].dir == ArduinoAPI::INPUT)
    {
        cout << "WARNING: Arduino digitalWrite() PULL-UP unsupported on pin: " << pin << endl;
        return;
    }

    d[pin].odat = val;
    if (d[pin].pwm_state == PWM_ENABLED)
    {
        d[pin].pwm_state = PWM_DISABLED;
    }

    Arduino::current = this;
}

int Arduino::digitalRead(int pin)
{
	cout << "Arduino [" << name() << "]: digitalRead() @ " << sc_time_stamp() << endl;
	TICK;

    if (pin > 13)
    {
        cout << "WARNING: Arduino digitalRead() unsupported pin: " << pin << endl;
        return -1;
    }

    Arduino::current = this;

    switch (Arduino::d[pin].sig->read().value())
    {
    case sc_dt::Log_0:
        return ArduinoAPI::LOW;
    case sc_dt::Log_1:
        return ArduinoAPI::HIGH;
    case sc_dt::Log_X:
        cout << "WARNING: Arduino digitalRead() unknown value at pin: " << pin << endl;
        return -1;
    case sc_dt::Log_Z:
        cout << "WARNING: Arduino digitalRead() floating line at pin: " << pin << endl;
        return -1;
    default:
        cout << "WARNING: Arduino digitalRead() unsupported value at pin: " << pin << endl;
        return -1;
    }
}

int Arduino::analogRead(int pin) 
{
    int val = -1;
    cout << "Arduino [" << name() << "]: analogRead() @ " << sc_time_stamp() << endl;
    TICK;

    if (pin > 5)
    {
        cout << "WARNING: Arduino analogRead() unsupported pin: " << pin << endl;
        return -1;
    }

    app_state_type state_saved = app_state.read();
    app_state.write(APP_STATE_INDELAY);
    wait(100, SC_US);	// based on Arduino documentation
    TICK;

    val = Arduino::a[pin]->read().value();

    app_state.write(state_saved);
    TICK;
    Arduino::current = this;
    return val;
}

void Arduino::analogWrite(int pin, int val)
{
    cout << "Arduino [" << name() << "]: analogWrite() @ " << sc_time_stamp() << endl;
    TICK;

    if (pin > 13 || d[pin].pwm_state == PWM_NOT_AVAILABLE) {
        cout << "WARNING: Arduino analogWrite() unsupported pin: " << pin << endl;
        return;
    }

    pinMode(pin, ArduinoAPI::OUTPUT);
    d[pin].dir = ArduinoAPI::OUTPUT;
    if (val == 0)
    {
        d[pin].pwm_state = PWM_DISABLED;
        d[pin].pwm_duty = val;
        digitalWrite(pin, ArduinoAPI::LOW);
    }
    else if (val == 255)
    {
        d[pin].pwm_state = PWM_DISABLED;
        d[pin].pwm_duty = val;
        digitalWrite(pin, ArduinoAPI::HIGH);
    }
    else
    {
        d[pin].pwm_state = PWM_ENABLED;
        d[pin].pwm_duty = max(min(val, Arduino::pwm_period), 1); 
    }

    Arduino::current = this;
}

unsigned long Arduino::millis()
{
	cout << "Arduino [" << name() << "]: millis() @ " << sc_time_stamp() << endl;
	TICK;
    return (unsigned long)((sc_time_stamp() - reset_time).to_seconds() * 1e3);
}

unsigned long Arduino::micros()
{
	cout << "Arduino [" << name() << "]: micros() @ " << sc_time_stamp() << endl;
	TICK;
    return (unsigned long)((sc_time_stamp() - reset_time).to_seconds() * 1e6);
}

void Arduino::delay(int ms)
{
	cout << "Arduino [" << name() << "]: delay() @ " << sc_time_stamp() << endl;
	app_state_type state_saved = app_state.read();
	app_state.write(APP_STATE_INDELAY);
	TICK;
	wait(ms, SC_MS);
	app_state.write(state_saved);
	TICK;
	Arduino::current = this;
}

void Arduino::delayMicroseconds(unsigned int us)
{
	cout << "Arduino [" << name() << "]: delayMicroseconds() @ " << sc_time_stamp() << endl;
	app_state_type state_saved = app_state.read();
	app_state.write(APP_STATE_INDELAY);
	TICK;
	wait(us, SC_US);
	app_state.write(state_saved);
	TICK;
	Arduino::current = this;
}

int	Arduino::serial_read() {
	cout << "Arduino [" << name() << "]: Serial.read() @ " << sc_time_stamp() << endl;
	TICK;
	int ch = rx_data.read();
	rx_data_rd.write(true);
	TICK;
	rx_data_rd.write(false);
	TICK;
	Arduino::current = this;
	return ch;
}

int	Arduino::serial_available() {
	TICK;
	Arduino::current = this;
	return rx_avail.read();
}

void Arduino::serial_write(int val) {
	cout << "Arduino [" << name() << "]: Serial.write() @ " << sc_time_stamp() << endl;
	TICK;
	tx_data.write(val);
	tx_data_wr.write(true);
	TICK;
	tx_data_wr.write(false);
	TICK;
	Arduino::current = this;
}

void Arduino::serial_write(const char * text) {
	if( NULL != text )
	{
		cout << "Arduino [" << name() << "]: Serial.write( \"" << text << "\" ) @ " << sc_time_stamp() << endl;
		for( int c = *text; 0 != c; c = *++text )
		{
			serial_write( c );
		}
	}
}


Arduino::Arduino(sc_module_name _name, ArduinoAPI::setupfn _setup, ArduinoAPI::loopfn _loop) :
	sc_module(_name), setup(_setup), loop(_loop), clk("clk", 200.84, SC_US)
{
	i_por = new por("POR");
	i_por->rst(rst);

	SC_THREAD(main);
	SC_METHOD(gpio_update);
		sensitive << clk.posedge_event();

    d[0].sig = &d0;
    d[1].sig = &d1;
    d[2].sig = &d2;
    d[3].sig = &d3; // PWM+
    d[4].sig = &d4;
    d[5].sig = &d5; // PWM+
    d[6].sig = &d6; // PWM+
    d[7].sig = &d7;
    d[8].sig = &d8;
    d[9].sig = &d9; // PWM
    d[10].sig = &d10; // PWM
    d[11].sig = &d11; // PWM
    d[12].sig = &d12;
    d[13].sig = &d13;

    a[0] = &a0;
    a[1] = &a1;
    a[2] = &a2;
    a[3] = &a3;
    a[4] = &a4;
    a[5] = &a5;

    pwm_cnt = 0;
    for (int i = 0; i < 14; i++)
    {
        d[i].pwm_duty = 1;
        d[i].dir = ArduinoAPI::INPUT;
        switch (i)
        {
        case 3:
        case 5:
        case 6:
        case 9:
        case 10:
        case 11:
            d[i].pwm_state = PWM_DISABLED;
            break;
        default:
            d[i].pwm_state = PWM_NOT_AVAILABLE;
        }
    }

    // UDP socket
    if (WSAStartup(MAKEWORD(2,2), &wsaData) != 0) {
        print_socket_error("Arduino: WinSock Initialization failed: ");
        WSACleanup();
        exit(WSAGetLastError());
    }

    udp_socket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);
    if (udp_socket == INVALID_SOCKET) {
        print_socket_error("Arduino: Server socket creation failed: ");
        WSACleanup();
        exit(WSAGetLastError());
    }

    SOCKADDR_IN server_inf;
    memset((void *)&server_inf, 0, sizeof(server_inf));
    server_inf.sin_family = AF_INET;
    server_inf.sin_addr.s_addr = inet_addr("127.0.0.1");
    server_inf.sin_port = htons(ARDUINO_GUI_PORT);

    if (connect(udp_socket, (SOCKADDR*)&(server_inf), sizeof(server_inf)) == SOCKET_ERROR) {
        print_socket_error("Arduino: Unable to connect to socket: ");
        closesocket(udp_socket);
        WSACleanup();
        exit(WSAGetLastError());
    }

    cout << "UDP socket set up was successful" << endl;

    app_state.initialize(APP_STATE_PRESETUP);
}

Arduino::~Arduino()
{
	if (i_por) {
		delete i_por;
	}
    closesocket(udp_socket);
    WSACleanup();
}

void  Arduino::gpio_update()
{
    sc_dt::sc_logic val;
    val = sc_dt::Log_Z;

    for (int i = 0; i < 14; i++)
    {
        switch (d[i].dir)
        {
        case ArduinoAPI::INPUT:
            val = sc_dt::Log_Z;
            break;
        case ArduinoAPI::OUTPUT:
            switch (d[i].pwm_state)
            {
            case PWM_ENABLED:
                val = pwm_cnt < d[i].pwm_duty ? sc_dt::Log_1 : sc_dt::Log_0;
                break;
            case PWM_DISABLED:
            case PWM_NOT_AVAILABLE:
                val = d[i].odat ? sc_dt::Log_1 : sc_dt::Log_0;
                break;
            default:
                cout << "WARNING: Arduino gpio_update() invalid PWM state" << endl;
            }
        }

        if (d[i].sig->read().value() != val)
        {
            d[i].sig->write(val);

            sprintf(send_buf, "%s %d",
                val == sc_dt::Log_Z ? "tri" : val == sc_dt::Log_0 ? "off" : "on", i);

            if (send(udp_socket, send_buf, (int)strlen(send_buf), 0) == SOCKET_ERROR) {
                print_socket_error("Arduino: Unable to send through the socket: ");
                WSACleanup();
                exit(WSAGetLastError());
            }
        }

    }
    pwm_cnt = (pwm_cnt + 1) % pwm_period;
}

void Arduino::main() {
	while (!rst.read()) wait(rst.value_changed_event());
	
	wait(clk.posedge_event());
	cout << "Arduino [" << name() << "]: Enter setup() @ " << sc_time_stamp() << endl;
    reset_time = sc_time_stamp();
	current = this;
	app_state.write(APP_STATE_INSETUP);
	setup();
	app_state.write(APP_STATE_IDLE);
	while (true) {
		wait(clk.posedge_event());
		//cout << "Arduino [" << name() << "]: Enter loop() @ " << sc_time_stamp() << endl;
		current = this;
		app_state.write(APP_STATE_INLOOP);
		loop();
		wait(20, SC_US);	// arbitrary estimate for loop execution time
		app_state.write(APP_STATE_IDLE);
		wait(10, SC_US);	// arbitrary estimate for scheduler overhead		
	}
}

