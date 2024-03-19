#include "ConfigUtils.h"
#include "AdvancedConfig.h"

#include <mutex>

#define SERIAL_BAUD_RATE 115200

//ANALOG INPUT CONFIG
#define USING_SPLAY false //whether or not your glove tracks splay. - tracks the side to side "wag" of fingers. Requires 5 more inputs.
#define FLIP_FLEXION  false  //Flip values from potentiometers (for fingers!) if they are backwards
#define FLIP_SPLAY true //Flip values for splay

#define INVERT_CALIB false

#define FLIP_FORCE_FEEDBACK true
#define SERVO_SCALING false //dynamic scaling of servo motors

//Select pins for multiplexers, set as needed if using a mux. You can add or remove pins as needed depending on how many select pins your mux needs.
#define PINS_MUX_SELECT     13,  /*S0 pin*/ \
                            12,  /*S1 pin*/ \
                            14,  /*S2 pin*/ \
                            27   /*S3 pin (if your mux is 3-bit like 74HC4051 then you can remove this line and the backslash before it.)*/

#define MUX_INPUT 35  //the input or SIG pin of the multiplexer. This can't be a mux pin.

#define PIN_PINKY     MUX(14) //These 5 are for flexion
#define PIN_RING      MUX(11)
#define PIN_MIDDLE    MUX(8)
#define PIN_INDEX     MUX(5)
#define PIN_THUMB     MUX(2)

#define PIN_PINKY_SECOND     MUX(13) 
#define PIN_RING_SECOND      MUX(10)
#define PIN_MIDDLE_SECOND    MUX(7)
#define PIN_INDEX_SECOND     MUX(4)
#define PIN_THUMB_SECOND     MUX(1)

#define PIN_CALIB     32 //button for recalibration
#define DEBUG_LED      2

#define PIN_PINKY_SERVO     19  //used for force feedback
#define PIN_RING_SERVO      18 //^
#define PIN_MIDDLE_SERVO     5 //^
#define PIN_INDEX_SERVO     17 //^
#define PIN_THUMB_SERVO     16 //^

#define PIN_PINKY_HAPTIC     23
#define PIN_RING_HAPTIC      22
#define PIN_MIDDLE_HAPTIC    15
#define PIN_INDEX_HAPTIC     4
#define PIN_THUMB_HAPTIC     21

#define PIN_THERMAL_IN1      33
#define PIN_THERMAL_IN2      25
#define PIN_THERMAL_ENA      26
#define THERMAL_PWM_CHANNEL  15

#define ALWAYS_CALIBRATING CALIBRATION_LOOPS == -1

bool calibrate = false;
bool calibButton = false;
bool isUsingFeedback = true;
int* fingerPos = (int[]){0,0,0,0,0,0,0,0,0,0};

ordered_lock* fingerPosLock = new ordered_lock();
TaskHandle_t Task1;
int threadLoops = 1;
int totalLocks = 0;
int lastMicros = 0;
int fullLoopTime = 0;
int fullLoopTotal = 0;
void getInputs(void* parameter)
{
    for(;;)
    {
        fullLoopTime = micros() - lastMicros;
        fullLoopTotal += fullLoopTime;
        lastMicros = micros();
        {
            fingerPosLock->lock();
            totalLocks++;
            getFingerPositions(calibrate, calibButton); //Save finger positions in thread
            fingerPosLock->unlock();
        }
        threadLoops++;
        if (threadLoops%100 == 0)
        {
            vTaskDelay(1); //keep watchdog fed
        }
        delayMicroseconds(1);
    }           
}

int loops = 0;
void setup() 
{
    pinMode(DEBUG_LED, OUTPUT);
    digitalWrite(DEBUG_LED, HIGH);

    serialStart();
    setupInputs();
    setupServos();
    setupThermal();
    setupHaptics();

    xTaskCreatePinnedToCore(
      getInputs, /* Function to implement the task */
      "Get_Inputs", /* Name of the task */
      10000,  /* Stack size in words */
      NULL,  /* Task input parameter */
      tskIDLE_PRIORITY,  /* Priority of the task */
      &Task1,  /* Task handle. */
      0); /* Core where the task should run */
    
}

int lastMainMicros = micros();
int mainMicros = 0;
int mainMicrosTotal = 0;
int mainloops = 1;

int target = 0;
bool latch = false;

unsigned long hapticTime[5] = {0,0,0,0,0};

void loop() 
{
    mainloops++;
    mainMicros = micros() - lastMainMicros;
    mainMicrosTotal += mainMicros;
    lastMainMicros = micros();

    if (!digitalRead(13))
    {
        if (!latch)
        {
            target++;
            target %= 5;
            latch = true;
        }
    }
    else
    {
        latch = false;
    }

    if (comOpen())
    {
        calibButton = getButton(PIN_CALIB) != INVERT_CALIB;
        if (calibButton)
        {   
            loops = 0;
        }
           
        if (loops < CALIBRATION_LOOPS || ALWAYS_CALIBRATING)
        {
            calibrate = true;
            loops++;
        }
        else
        {
            calibrate = false;
        }

        bool triggerButton = triggerGesture(fingerPos);
        bool grabButton = grabGesture(fingerPos);
        bool pinchButton = pinchGesture(fingerPos);

        int fingerPosCopy[10];
        int mutexTimeDone;
        {
            int mutexTime = micros();
            fingerPosLock->lock();
            mutexTimeDone = micros()-mutexTime;
            for (int i = 0; i < 10; i++)
            {
                fingerPosCopy[i] = fingerPos[i];
            }
            fingerPosLock->unlock();
        }

        serialOutput(encodeData(fingerPosCopy, triggerButton, grabButton, pinchButton));

        char received[100];
        if(serialRead(received))
        {
            int parsedThermal[1];
            int parsedServo[5];
            int parsedHaptic[5];
            if(decodeData(received, parsedThermal, parsedServo, parsedHaptic))
            {
                writeThermal(parsedThermal);
                writeServos(parsedServo);
                writeHaptics(parsedHaptic);
            }
        }
        checkHaptics();
    }
    delay(LOOP_TIME);
}
