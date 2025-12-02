#include <WiFi.h>
#include <vector>
#include <map>

// Sound sensor configuration
const int soundPin = 34;
const int MIN_SOUND = 1395;
const int MAX_SOUND = 2000;

// WiFi monitoring configuration
const char* targetSSID = "STLHVF3V";
unsigned long lastWiFiScan = 0;
unsigned long lastPeopleScan = 0;
const unsigned long wifiScanInterval = 5000;      // Scan target WiFi every 5 seconds
const unsigned long peopleScanInterval = 15000;   // Scan for people every 15 seconds

// Device Type Detection Classes
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
    struct DeviceInfo {
        String mac;
        String type;
        int rssi;
        unsigned long firstSeen;
        unsigned long lastSeen;
        int scanCount;
    };

    std::vector<DeviceInfo> deviceHistory;
    DeviceTypeDetector typeDetector;

public:
    void analyzeDevices() {
        int n = WiFi.scanNetworks(false, true);
        
        if (n <= 0) {
            Serial.println("No devices detected");
            return;
        }

        Serial.println("\n=== People Detection Analysis ===");
        
        int computerCount = 0;
        int phoneCount = 0;
        int routerCount = 0;
        int unknownCount = 0;

        for (int i = 0; i < n; ++i) {
            String mac = WiFi.BSSIDstr(i);
            int rssi = WiFi.RSSI(i);
            String ssid = WiFi.SSID(i);
            
            String deviceType = classifyDevice(mac, rssi, ssid);
            
            Serial.print("Device ");
            Serial.print(i+1);
            Serial.print(": ");
            Serial.print(deviceType);
            Serial.print(" (");
            Serial.print(rssi);
            Serial.println(" dBm)");
            
            // Statistics
            if (deviceType.indexOf("Computer") != -1 || 
                deviceType.indexOf("MacBook") != -1) {
                computerCount++;
            } else if (deviceType.indexOf("Phone") != -1 || 
                       deviceType.indexOf("iPhone") != -1) {
                phoneCount++;
            } else if (deviceType.indexOf("Router") != -1) {
                routerCount++;
            } else {
                unknownCount++;
            }
        }

        Serial.println("--- Summary ---");
        Serial.print("Computers: ");
        Serial.print(computerCount);
        Serial.print(" | Phones: ");
        Serial.print(phoneCount);
        Serial.print(" | Routers: ");
        Serial.println(routerCount);
        Serial.print("Estimated People: ");
        Serial.println(estimatePeople(phoneCount, computerCount));
        Serial.println("=================================\n");
        
        WiFi.scanDelete();
    }

    String classifyDevice(const String& mac, int rssi, const String& ssid) {
        String vendor = typeDetector.getVendorFromMAC(mac);
        
        if (isLikelyRouter(ssid)) {
            return "WiFi Router/Access Point";
        }
        
        if (rssi > -35) {
            return "Likely Computer (Very Strong Signal)";
        }
        
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
        
        if (ssid.length() == 0 || ssid == "hidden") {
            return true;
        }
        
        return false;
    }

    // Assume that each person has at least one phone, and one laptop.
    // Use the larger number as the estimated people counts. If 1 phone and 2 computers, then 2 people; if 3 phones and 2 computer, than 3 people. 
    int estimatePeople(int phoneCount, int computerCount) const {
        int estimatedFromPhones = phoneCount;
        int estimatedFromComputers = computerCount; 
        
        return (estimatedFromPhones > estimatedFromComputers) ? estimatedFromPhones : estimatedFromComputers;
    }
};

// Global instances
EnhancedPeopleCounter peopleCounter;

void setup() {
  Serial.begin(115200);
  while (!Serial);
  
  // Setup ADC for sound sensor
  analogSetAttenuation(ADC_11db);
  
  // Setup WiFi for scanning
  WiFi.mode(WIFI_STA);
  WiFi.disconnect(true); 
  delay(1000);
  
  Serial.println("=================================");
  Serial.println("Multi-Sensor Monitoring System");
  Serial.println("- Sound Level Monitor");
  Serial.println("- WiFi Signal Monitor (STLHVF3V)");
  Serial.println("- People Detection System");
  Serial.println("=================================\n");
}

void loop() {
  // Read sound level continuously
  int soundValue = analogRead(soundPin);
  soundValue = constrain(soundValue, MIN_SOUND, MAX_SOUND);
  float dB_SPL = map(soundValue, MIN_SOUND, MAX_SOUND, 30, 90);
  
  Serial.print("Sound: ");
  Serial.print(dB_SPL);
  Serial.println(" dB");
  
  // Scan for target WiFi periodically
  if (millis() - lastWiFiScan >= wifiScanInterval) {
    lastWiFiScan = millis();
    scanTargetWiFi();
  }
  
  // Scan for people periodically
  if (millis() - lastPeopleScan >= peopleScanInterval) {
    lastPeopleScan = millis();
    peopleCounter.analyzeDevices();
  }
  
  delay(50);
}

void scanTargetWiFi() {
  Serial.println("\n--- WiFi Monitor (STLHVF3V) ---");
  int n = WiFi.scanNetworks();
  
  bool found = false;
  
  for (int i = 0; i < n; i++) {
    if (WiFi.SSID(i) == targetSSID) {
      Serial.print("SSID: ");
      Serial.print(WiFi.SSID(i));
      Serial.print(" | RSSI: ");
      Serial.print(WiFi.RSSI(i));
      Serial.println(" dBm");
      found = true;
      break;
    }
  }
  
  if (!found) {
    Serial.println("STLHVF3V not found");
  }
  
  Serial.println("-------------------------------\n");
}
