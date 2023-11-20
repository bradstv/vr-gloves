bool comIsOpen = false;

bool comOpen()
{
    return comIsOpen;
}

void serialStart()
{
    Serial.begin(SERIAL_BAUD_RATE);
    comIsOpen = true;
}

void serialOutput(char* data)
{
    Serial.print(data);
    Serial.flush();
}

bool serialRead(char* input)
{
    byte size = Serial.readBytesUntil('\n', input, 100);
    input[size] = NULL; //terminate end of string with null
    return input != NULL && strlen(input) > 0;
}
