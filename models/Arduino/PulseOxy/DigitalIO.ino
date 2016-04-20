/*
  Pulse Oxymeter firmware example
  
  Copyright (c) 2014 MetaMorph, Inc.
  All Rights Reserved.
  
  Author: Peter Volgyesi <pvolgyesi@metamorphsoftware.com>
 
 */
 

int ledPin = 1;

void setup()
{
  pinMode(ledPin, OUTPUT);      // sets the digital pin as output
}

void loop()
{
  digitalWrite(ledPin, HIGH);   // sets the LED on
  delay(1);                     // waits for a millisecond
  digitalWrite(ledPin, LOW);    // sets the LED off
  delay(1);                     // waits for a millisecond
}
