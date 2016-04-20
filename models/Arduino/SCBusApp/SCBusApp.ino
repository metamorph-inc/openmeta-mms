/*
  SCBus firmware example

  Copyright (c) 2014 MetaMorph, Inc.
  All Rights Reserved.

  Author: Peter Volgyesi <pvolgyesi@metamorphsoftware.com>

 */

const int analog0Pin = 0;
const int analog1Pin = 1;
const int pwm0Pin = 9;
const int pwm1Pin = 10;
const int pwm2Pin = 11;

const int ANALOG_MAX = 1023;
const int ANALOG_MIN = 0;
const int PWM_MAX = 255;

void setup() {
  Serial.begin(9600);
}

void loop() {
  if (Serial.available() > 0) {
    int inByte = Serial.read();

    // Use incoming value as PWM duty cycle
    analogWrite(pwm0Pin, inByte);

    // Send back some data
    Serial.write(inByte + 1);
  }
}
