#include <WiFi.h>
#include <vector>
#include <map>
#include "time.h"
#include <PubSubClient.h>
#include "arduino_secrets.h" 
#include "soc/soc.h"
#include "soc/rtc_cntl_reg.h"

// --- Configuration ---
// 1. WiFi Config (Your Hotspot)
const char* internet_ssid     = SECRET_SSID; 
const char* internet_password = SECRET_PASS; 

// 2. MQTT Server Config
const char* mqtt_server = "mqtt.cetools.org";
const int   mqtt_port   = 1884;
const char* mqtt_topic  = "student/group_laxr/json_data";
const char* mqtt_user   = "student";
const char* mqtt_pass   = "ce2021-mqtt-forget-whale";

// 3. Target WiFi to Track
const char* targetSSID = "eduroam";

// 4. Timezone
const long  gmtOffset_sec = 0 * 3600;
const int   daylightOffset_sec = 0;

// Sensor Config
const int soundPin = 34;
const int MIN_SOUND = 1395;
const int MAX_SOUND = 2000;

const char* ntpServer = "pool.ntp.org";

// --- Global variables ---
int globalPeopleCount = 0;
int globalTargetRSSI = -100;
int globalComputerCount = 0;
int globalPhoneCount = 0;

// --- Class Definitions ---
class DeviceTypeDetector {
private:
    std::map<String, String> macVendors = {
        {"A8:5B:F7", "Apple"},
        {"24:E1:24", "Apple"},
        {"8C:85:80", "Apple"},
        {"DC:A6:32", "Raspberry Pi"},
        {"B8:27:EB", "Raspberry Pi"},
        {"00:50:F1", "Dell"},
        {"00:1A:11", "Google"},
        {"38:F9:D3", "Google"},
        {"44:07:0B", "Huawei"},
        {"AC:BC:32", "Samsung"},
        {"34:BB:1F", "Microsoft"},
        {"98:0C:82", "Sony"},
        {"C0:EE:FB", "OnePlus"},
        {"14:F6:5A", "Xiaomi"},
        {"08:EE:8B", "Netgear"}
    };

public:
    String detectDeviceType(const String& mac, int rssi) const {
        String vendor = getVendorFromMAC(mac);
        
        if (vendor == "Apple") {
            return guessAppleDeviceType(mac, rssi);
        } else if (vendor != "Unknown") {
            return vendor + " Device";
        }
        
        return guessBySignalCharacteristics(mac, rssi);
    }

    String getVendorFromMAC(const String& mac) const {
        String prefix = mac.substring(0, 8);
        prefix.toUpperCase(); 
        for (const auto& vendor : macVendors) {
            if (prefix.startsWith(vendor.first)) {
                return vendor.second;
            }
        }
        return "Unknown";
    }

    String guessAppleDeviceType(const String& mac, int rssi) const {
        int macSuffix = getMacSuffix(mac);
        
        if (rssi > -45 && macSuffix % 2 == 0) {
            return "Apple iPhone";
        } else if (rssi > -50) {
            return "Apple MacBook";
        } else {
            return "Apple Device";
        }
    }

    String guessBySignalCharacteristics(const String& mac, int rssi) const {
        if (rssi > -40) {
            return "Likely Computer (Strong Signal)";
        } else if (rssi > -55) {
            return "Possible Phone (Medium Signal)";
        } else {
            return "Mobile Device (Weak Signal)";
        }
    }

private:
    int getMacSuffix(const String& mac) const {
        String lastPart = mac.substring(15);
        return (int)strtol(lastPart.c_str(), NULL, 16);
    }
};

class EnhancedPeopleCounter {
private:
    DeviceTypeDetector typeDetector;

public:
    void analyzeDevices(const char* target_ssid) {
        int n = WiFi.scanNetworks(false, true);
        
        globalTargetRSSI = -100;

        if (n == 0) {
            Serial.println("No networks found.");
            globalPeopleCount = 0;
            globalComputerCount = 0; 
            globalPhoneCount = 0;    
            return;
        } else if (n < 0) {
            Serial.println("Scan error.");
            return;
        }

        Serial.print("\n=== Scan Complete: Found ");
        Serial.print(n);
        Serial.println(" networks ===");
        
        int computerCount = 0;
        int phoneCount = 0;
        int routerCount = 0;
        int unknownCount = 0;

        for (int i = 0; i < n; ++i) {
            String mac = WiFi.BSSIDstr(i);
            int rssi = WiFi.RSSI(i);
            String ssid = WiFi.SSID(i);
            
            // Update Target RSSI if found
            if (ssid == String(target_ssid)) {
                if (rssi > globalTargetRSSI) {
                    globalTargetRSSI = rssi;
                }
            }

            String deviceType = classifyDevice(mac, rssi, ssid);
            
            // Debug print
            Serial.print(ssid);
            Serial.print(" ["); Serial.print(mac); Serial.print("] ");
            Serial.print(rssi); Serial.print("dBm -> ");
            Serial.println(deviceType);
            
            if (deviceType.indexOf("Computer") != -1 || deviceType.indexOf("MacBook") != -1) {
                computerCount++;
            } else if (deviceType.indexOf("Phone") != -1 || deviceType.indexOf("iPhone") != -1) {
                phoneCount++;
            } else if (deviceType.indexOf("Router") != -1) {
                routerCount++;
            } else {
                unknownCount++;
            }
        }

        // Update Global Variables
        globalPeopleCount = estimatePeople(phoneCount, computerCount);
        globalComputerCount = computerCount;
        globalPhoneCount = phoneCount;
        
        WiFi.scanDelete(); 
    }

    String classifyDevice(const String& mac, int rssi, const String& ssid) {
        String vendor = typeDetector.getVendorFromMAC(mac);
        
        if (ssid.length() > 0 && ssid != "hidden") {
             if (isLikelyRouter(ssid)) return "WiFi Router/Access Point";
        }
        
        if (rssi > -35) return "Likely Computer (Very Strong Signal)";
        
        if (vendor == "Apple") {
            return typeDetector.guessAppleDeviceType(mac, rssi);
        }
        
        if (rssi > -60 && rssi < -40) {
            return "Likely Mobile Phone";
        }
        
        return "Unknown Device";
    }

    bool isLikelyRouter(const String& ssid) const {
        const char* commonRouterPatterns[] = {
            "TP-Link", "Netgear", "ASUS", "Linksys", 
            "DIRECT", "Xiaomi", "Huawei", "Tenda",
            "MERCURY", "FAST", "dlink", "Belkin"
        };
        
        String lowerSSID = ssid;
        lowerSSID.toLowerCase();
        
        for (int i = 0; i < 12; i++) {
            String pattern = String(commonRouterPatterns[i]);
            pattern.toLowerCase();
            if (lowerSSID.indexOf(pattern) != -1) {
                return true;
            }
        }
        
        return false;
    }

    int estimatePeople(int phoneCount, int computerCount) const {
        return (phoneCount > computerCount) ? phoneCount : computerCount;
    }
};

// --- Object Instantiation ---
EnhancedPeopleCounter peopleCounter;

WiFiClient espClient;           
PubSubClient client(espClient); 

// --- Variables ---
unsigned long lastReportTime = 0;
const unsigned long reportInterval = 5000; 
unsigned long lastSampleTime = 0;
const unsigned long sampleInterval = 50;   
long soundSum = 0;                         
int sampleCount = 0;                       

// --- Function Declarations ---
void setup_wifi();
String getRealTime();

// --- Setup ---
void setup() {
  WRITE_PERI_REG(RTC_CNTL_BROWN_OUT_REG, 0);

  Serial.begin(115200);
  delay(1000);

  analogSetAttenuation(ADC_11db);

  Serial.println("System Starting...");

  // 1. Connect WiFi
  setup_wifi();

  // 2. Get Time
  configTime(gmtOffset_sec, daylightOffset_sec, ntpServer);
  struct tm timeinfo;
  if (getLocalTime(&timeinfo)) {
    Serial.println("Time synced!");
  } else {
    Serial.println("Time sync failed (will retry)");
  }

  // 3. Config MQTT Server
  client.setServer(mqtt_server, mqtt_port);
  
  // 4. Initial MQTT Connection
  Serial.print("Connecting to MQTT...");
  String clientId = "ESP32-" + String(random(0xffff), HEX); 
  
  if (client.connect(clientId.c_str(), mqtt_user, mqtt_pass)) {
    Serial.println("connected");
  } else {
    Serial.print("failed, rc=");
    Serial.println(client.state());
  }
}

// --- Loop ---
void loop() {
  if (!client.connected()) {
    Serial.print("MQTT Disconnected! Reconnecting...");
    String clientId = "ESP32-" + String(random(0xffff), HEX);
    
    if (client.connect(clientId.c_str(), mqtt_user, mqtt_pass)) {
      Serial.println("connected");
    } else {
      Serial.print("failed, rc=");
      Serial.println(client.state());
      delay(2000); 
      return;      
    }
  }
  
  client.loop(); 

  unsigned long currentMillis = millis();

  // 1. Sound Sampling
  if (currentMillis - lastSampleTime >= sampleInterval) {
    lastSampleTime = currentMillis;
    int rawValue = analogRead(soundPin);
    int constrainedVal = constrain(rawValue, MIN_SOUND, MAX_SOUND);
    float instantaneousDB = map(constrainedVal, MIN_SOUND, MAX_SOUND, 30, 90);
    soundSum += instantaneousDB;
    sampleCount++;
  }

  // 2. Data Processing & Reporting
  if (currentMillis - lastReportTime >= reportInterval) {
    
    float avgSoundDB = 0;
    if (sampleCount > 0) {
      avgSoundDB = (float)soundSum / sampleCount;
    }

    Serial.println("\n--- Starting Scan ---");
    // Analyze Devices
    peopleCounter.analyzeDevices(targetSSID);

    String timeStr = getRealTime();

    String jsonOutput = "{";
    jsonOutput += "\"time\":\"" + timeStr + "\",";
    jsonOutput += "\"sound_db\":" + String(avgSoundDB, 1) + ",";
    jsonOutput += "\"wifi_rssi\":" + String(globalTargetRSSI) + ",";
    jsonOutput += "\"people_count\":" + String(globalPeopleCount) + ",";
    jsonOutput += "\"computer_count\":" + String(globalComputerCount) + ",";
    jsonOutput += "\"phone_count\":" + String(globalPhoneCount);
    jsonOutput += "}";

    Serial.println("Publishing: " + jsonOutput);
    
    client.publish(mqtt_topic, jsonOutput.c_str());

    soundSum = 0;
    sampleCount = 0;
    lastReportTime = millis();
  }
}

// --- Helpers ---
void setup_wifi() {
  delay(10);
  Serial.println();
  Serial.print("Connecting to ");
  Serial.println(internet_ssid);
  WiFi.mode(WIFI_STA);
  WiFi.begin(internet_ssid, internet_password);
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }
  Serial.println(" WiFi connected");
}

String getRealTime() {
  struct tm timeinfo;
  if(!getLocalTime(&timeinfo)){
    return "Time Error";
  }
  char timeStringBuff[50];
  strftime(timeStringBuff, sizeof(timeStringBuff), "%Y-%m-%d %H:%M:%S", &timeinfo);
  return String(timeStringBuff);
}