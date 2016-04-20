/*
  XmasTree firmware

  Copyright (c) 2014 MetaMorph, Inc.
  All Rights Reserved.

  Author: Sandor Szilvasi <sszilvasi@metamorphsoftware.com>

 */

static const int BUF_LEN = 512;
static const int LED_NUM = 14;
static const int BLINK_PER = 500; // blink half-period
static char buf[BUF_LEN];
static char arg[BUF_LEN];
static int ctr;
static int led;
static int blink_state[LED_NUM]; // 0: off 1: on

void setup() {
    //cout << "XmasTree firmware: setup() invoked" << endl;
    ctr = 0;

    for (int i = 0; i < LED_NUM; i++) {
        pinMode(i, OUTPUT);
        blink_state[i] = 0;
    }

    Serial.begin(9600);
}

void loop() {
    //cout << "XmasTree firmware: loop() invoked" << endl;
    while (Serial.available() > 0) {
        int inByte = Serial.read();

        buf[ctr++] = inByte & 0xFF;
        cout << "XmasTree firmware: new BYTE received" << endl;

        if (inByte == '\0')
        {
            ctr = 0;
            cout << "XmasTree firmware: new message received" << endl;
            cout << buf << endl;

            for (int i = 0; i < BUF_LEN && buf[i]; i++) {
                buf[i] = tolower(buf[i]);
            }

            if (sscanf(buf,"led%d %s", &led, arg) == 2)
            {
                if (led >= LED_NUM) {
                    cout << "WARNING: LED index " << led << " is out of range 0..13" << endl;
                    return;
                }
                   
                if (strncmp(arg,"on",2) == 0) {
                    blink_state[led] = 0;
                    digitalWrite(led, HIGH);
                    return;
                }

                if (strncmp(arg,"off",3) == 0) {
                    blink_state[led] = 0;
                    digitalWrite(led, LOW);
                    return;
                }

                if (strncmp(arg,"blink",5) == 0) {
                    blink_state[led] = 1;
                    cout << "XmasTree firmware: blink " << led << endl;
                    return;
                }
            }
        }
    }

    // blink
    delay(BLINK_PER);
    for (int i = 0; i < LED_NUM; i++) {
        if (blink_state[i]) {
            cout << "Toggle LED " << i << endl;
            digitalWrite(i, digitalRead(i) ? LOW : HIGH);
        }
    }

}
