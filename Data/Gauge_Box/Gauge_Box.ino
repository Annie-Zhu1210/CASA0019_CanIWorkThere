#include <WiFiNINA.h>
#include <PubSubClient.h>
#include <ArduinoJson.h>
#include <Servo.h>
#include <Adafruit_NeoPixel.h>
#include "secrets.h"

// WiFi credentials
const char* ssid = SECRET_SSID;
const char* password = SECRET_PASS;

// MQTT broker settings
const char* mqtt_server = "mqtt.cetools.org";
const int mqtt_port = 1884;
const char* mqtt_user = SECRET_MQTT_USER;
const char* mqtt_password = SECRET_MQTT_PASS;
const char* mqtt_topic = "student/group_laxr/json_data";

// Define pins
const int servoPin = 3;
const int ledPin = 8;
const int numLEDs = 5;

// Create objects
WiFiClient wifiClient;
PubSubClient mqttClient(wifiClient);
Servo gaugeServo;
Adafruit_NeoPixel strip(numLEDs, ledPin, NEO_GRB + NEO_KHZ800);

// Define LED colours
uint32_t red = strip.Color(255, 0, 0);
uint32_t orange = strip.Color(255, 165, 0);
uint32_t yellow = strip.Color(255, 255, 0);
uint32_t yellowGreen = strip.Color(128, 255, 0);
uint32_t green = strip.Color(0, 255, 0);

void setup() {
  Serial.begin(115200);
  while (!Serial);
  
  Serial.println("Real-Time Gauge Display System");
  
  // Initialise servo
  gaugeServo.attach(servoPin);
  gaugeServo.write(0);
  Serial.println("Servo initialised on Pin 3");
  
  // Initialise LED strip
  strip.begin();
  strip.setBrightness(50);
  strip.show();
  Serial.println("LED strip initialised on Pin 8");
  
  // Connect to WiFi
  connectWiFi();
  
  // Setup MQTT
  mqttClient.setServer(mqtt_server, mqtt_port);
  mqttClient.setCallback(mqttCallback);
  
  Serial.println("\nSystem ready!");
}

void loop() {
  if (!mqttClient.connected()) {
    reconnectMQTT();
  }
  mqttClient.loop();
}

void connectWiFi() {
  Serial.print("Connecting to WiFi: ");
  Serial.println(ssid);
  
  WiFi.begin(ssid, password);
  
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }
  
  Serial.println("\nWiFi connected!");
  Serial.print("IP address: ");
  Serial.println(WiFi.localIP());
}

void reconnectMQTT() {
  while (!mqttClient.connected()) {
    Serial.print("Connecting to MQTT broker...");
    
    String clientId = "MKR1010-Gauge-";
    clientId += String(random(0xffff), HEX);
    
    if (mqttClient.connect(clientId.c_str(), mqtt_user, mqtt_password)) {
      Serial.println("connected!");
      mqttClient.subscribe(mqtt_topic);
      Serial.print("Subscribed to: ");
      Serial.println(mqtt_topic);
    } else {
      Serial.print("failed, rc=");
      Serial.print(mqttClient.state());
      Serial.println(" retrying in 5 seconds...");
      delay(5000);
    }
  }
}

void mqttCallback(char* topic, byte* payload, unsigned int length) {
  Serial.print("Message received on topic: ");
  Serial.println(topic);
  
  // Convert payload to string
  String message = "";
  for (unsigned int i = 0; i < length; i++) {
    message += (char)payload[i];
  }
  
  Serial.print("Payload: ");
  Serial.println(message);
  
  // Parse json
  StaticJsonDocument<200> doc;
  DeserializationError error = deserializeJson(doc, message);
  
  if (error) {
    Serial.print("JSON parsing failed: ");
    Serial.println(error.c_str());
    return;
  }
  
  // Extract sound_db and wifi_rssi
  if (doc.containsKey("sound_db")) {
    float sound_db = doc["sound_db"];
    updateServo(sound_db);
  }
  
  if (doc.containsKey("wifi_rssi")) {
    int wifi_rssi = doc["wifi_rssi"];
    updateLEDs(wifi_rssi);
  }
}

void updateServo(float dB) {
  // Map 30-90 dB to 0-270 degrees for the pointer
  int servoAngle = map(dB * 10, 300, 900, 0, 135);
  servoAngle = constrain(servoAngle, 0, 135);
  
  gaugeServo.write(servoAngle);
  
  int pointerAngle = servoAngle * 2;
  
  Serial.print("Sound Level: ");
  Serial.print(dB);
  Serial.print(" dB → Servo: ");
  Serial.print(servoAngle);
  Serial.print("° → Pointer: ");
  Serial.print(pointerAngle);
  Serial.println("°");
}

void updateLEDs(int rssi) {
  // Map -30 to -80 dBm to 5 LED levels
  // -30 to -40: 5 LEDs (green)
  // -40 to -50: 4 LEDs (yellow-green)
  // -50 to -60: 3 LEDs (yellow)
  // -60 to -70: 2 LEDs (orange)
  // -70 to -80: 1 LED (red)
  
  int level;
  uint32_t color;
  
  if (rssi >= -40) {
    level = 5;
    color = green;
  } else if (rssi >= -50) {
    level = 4;
    color = yellowGreen;
  } else if (rssi >= -60) {
    level = 3;
    color = yellow;
  } else if (rssi >= -70) {
    level = 2;
    color = orange;
  } else {
    level = 1;
    color = red;
  }
  
  // Clear all LEDs
  strip.clear();
  
  // Light up LEDs based on signal strength
  for (int i = 0; i < level; i++) {
    strip.setPixelColor(i, color);
  }
  
  strip.show();
  
  Serial.print("WiFi Signal: ");
  Serial.print(rssi);
  Serial.print(" dBm → ");
  Serial.print(level);
  Serial.println(" LEDs");
}