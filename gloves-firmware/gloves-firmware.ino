#define PIN_PINKY_BUZZER     32
#define PIN_RING_BUZZER      33
#define PIN_MIDDLE_BUZZER    25
#define PIN_INDEX_BUZZER     26
#define PIN_THUMB_BUZZER     27
#define PIN_THERMO_IN1       14
#define PIN_THERMO_IN2       12
#define PIN_THERMO_ENA       13
#define THERMO_PWM_CHANNEL   15
#define PIN_PINKY_SERVO       5
#define PIN_RING_SERVO       18
#define PIN_MIDDLE_SERVO     19
#define PIN_INDEX_SERVO      21
#define PIN_THUMB_SERVO      17
#define LOOP_TIME             4
#define SERIAL_BAUD_RATE     115200

void setup() 
{
    serialStart();
    setupServos();
    setupThermo();
    setupBuzzers();
}

void loop() 
{
    if(isComOpen())
    {
        char received[100];
        if(serialRead(received))
        {
            int parsedThermo[1];
            int parsedServo[5];
            bool parsedBuzzer[5];
            parseData(received, parsedThermo, parsedServo, parsedBuzzer);
            writeThermo(parsedThermo);
            writeServos(parsedServo);
            writeBuzzers(parsedBuzzer);
        }
        delay(LOOP_TIME);
    }

}
