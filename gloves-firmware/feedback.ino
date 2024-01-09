#include "ESP32Servo.h"

Servo pinkyServo;
Servo ringServo;
Servo middleServo;
Servo indexServo;
Servo thumbServo;

void setupServos()
{
    pinkyServo.attach(PIN_PINKY_SERVO);
    ringServo.attach(PIN_RING_SERVO);
    middleServo.attach(PIN_MIDDLE_SERVO);
    indexServo.attach(PIN_INDEX_SERVO);
    thumbServo.attach(PIN_THUMB_SERVO);
}

void setupThermo()
{
    pinMode(PIN_THERMO_IN1, OUTPUT);
    pinMode(PIN_THERMO_IN2, OUTPUT);
    
    ledcSetup(THERMO_PWM_CHANNEL, 1000, 8);
    ledcAttachPin(PIN_THERMO_ENA, THERMO_PWM_CHANNEL);
}

void setupBuzzers()
{
    pinMode(PIN_PINKY_BUZZER, OUTPUT);
    pinMode(PIN_RING_BUZZER, OUTPUT);
    pinMode(PIN_MIDDLE_BUZZER, OUTPUT);
    pinMode(PIN_INDEX_BUZZER, OUTPUT);
    pinMode(PIN_THUMB_BUZZER, OUTPUT);
}

void writeServos(int* parsedServo)
{
    float scaledLimits[5];
    mapServoLimits(parsedServo, scaledLimits);
    indexServo.write(scaledLimits[0]);
    middleServo.write(scaledLimits[1]);
    pinkyServo.write(scaledLimits[2]);
    ringServo.write(scaledLimits[3]);
    thumbServo.write(scaledLimits[4]);
}

void writeThermo(int* parsedThermo)
{
    int thermoValues[1];
    mapThermoValue(parsedThermo, thermoValues);

    if(thermoValues[0] == 0) //turn off
    {
        digitalWrite(PIN_THERMO_IN1, LOW);
        digitalWrite(PIN_THERMO_IN2, LOW); 
    }
    else if(thermoValues[0] < 0) //start cooling
    {
        digitalWrite(PIN_THERMO_IN1, HIGH);
        digitalWrite(PIN_THERMO_IN2, LOW);
        ledcWrite(THERMO_PWM_CHANNEL, abs(thermoValues[0])); 
    }
    else if(thermoValues[0] > 0) //start heating
    {
        digitalWrite(PIN_THERMO_IN1, LOW);
        digitalWrite(PIN_THERMO_IN2, HIGH);
        ledcWrite(THERMO_PWM_CHANNEL, thermoValues[0]);
    }
}

void writeBuzzers(int* parsedBuzzer)
{
    unsigned long currentTime = millis();
    if(parsedBuzzer[0] > 0)
    {   
        buzzerTime[0] = parsedBuzzer[0] + currentTime;
        digitalWrite(PIN_THUMB_BUZZER, HIGH);
    }

    if(parsedBuzzer[1] > 0)
    {   
        buzzerTime[1] = parsedBuzzer[1] + currentTime;
        digitalWrite(PIN_INDEX_BUZZER, HIGH);
    }

    if(parsedBuzzer[2] > 0)
    {   
        buzzerTime[2] = parsedBuzzer[2] + currentTime;
        digitalWrite(PIN_MIDDLE_BUZZER, HIGH);
    }

    if(parsedBuzzer[3] > 0)
    {   
        buzzerTime[3] = parsedBuzzer[3] + currentTime;
        digitalWrite(PIN_RING_BUZZER, HIGH);
    }

    if(parsedBuzzer[4] > 0)
    {   
        buzzerTime[4] = parsedBuzzer[4] + currentTime;
        digitalWrite(PIN_PINKY_BUZZER, HIGH);
    }
}

void checkBuzzers()
{
    unsigned long currentTime = millis();
    if(buzzerTime[0] > 0 && currentTime > buzzerTime[0])
    {
        buzzerTime[0] = 0;
        digitalWrite(PIN_THUMB_BUZZER, LOW);
    }

    if(buzzerTime[1] > 0 && currentTime > buzzerTime[1])
    {
        buzzerTime[1] = 0;
        digitalWrite(PIN_INDEX_BUZZER, LOW);
    }

    if(buzzerTime[2] > 0 && currentTime > buzzerTime[2])
    {
        buzzerTime[2] = 0;
        digitalWrite(PIN_MIDDLE_BUZZER, LOW);
    }

    if(buzzerTime[3] > 0 && currentTime > buzzerTime[3])
    {
        buzzerTime[3] = 0;
        digitalWrite(PIN_RING_BUZZER, LOW);
    }

    if(buzzerTime[4] > 0 && currentTime > buzzerTime[4])
    {
        buzzerTime[4] = 0;
        digitalWrite(PIN_PINKY_BUZZER, LOW);
    }
}


