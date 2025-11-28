// This is a testing code for the MAX9814 sound sensor using Arduino MKR WIFI 1010 board.

const int soundPin = A0;
const int MIN_SOUND = 1380;  // Quiet baseline
const int MAX_SOUND = 1800;  // Loud sounds

void setup() {
  Serial.begin(9600);
  while (!Serial);
  analogReadResolution(12);
}

void loop() {
  int soundValue = analogRead(soundPin);
  
  // Constrain to expected range
  soundValue = constrain(soundValue, MIN_SOUND, MAX_SOUND);
  
  // Map to calibrated dB SPL range
  float dB_SPL = map(soundValue, MIN_SOUND, MAX_SOUND, 33, 80);
  
  // Print only the dB value for Serial Plotter
  Serial.println(dB_SPL);
  
  delay(50);
}