#include <WiFi.h>
#include <vector>
#include <map>

class DeviceTypeDetector {
private:
    // MAC address vendor prefix database (partial)
    std::map<String, String> macVendors = {
        {"A8:5B:F7", "Apple"},        // Apple devices
        {"24:E1:24", "Apple"},        // Apple devices  
        {"8C:85:80", "Apple"},
        {"DC:A6:32", "Raspberry Pi"}, // Raspberry Pi
        {"B8:27:EB", "Raspberry Pi"},
        {"00:50:F1", "Dell"},         // Dell computers
        {"00:1A:11", "Google"},       // Google devices
        {"38:F9:D3", "Google"},
        {"44:07:0B", "Huawei"},       // Huawei
        {"AC:BC:32", "Samsung"},      // Samsung
        {"34:BB:1F", "Microsoft"},    // Microsoft
        {"98:0C:82", "Sony"},         // Sony
        {"C0:EE:FB", "OnePlus"},      // OnePlus
        {"14:F6:5A", "Xiaomi"},       // Xiaomi
        {"08:EE:8B", "Netgear"}       // Router
    };

public:
    String detectDeviceType(const String& mac, int rssi) const {
        String vendor = getVendorFromMAC(mac);
        
        if (vendor == "Apple") {
            // Apple devices could be iPhone, MacBook, iPad, etc.
            return guessAppleDeviceType(mac, rssi);
        } else if (vendor != "Unknown") {
            return vendor + " Device";
        }
        
        // Guess based on signal characteristics
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
        // Simple classification logic for Apple devices
        int macSuffix = getMacSuffix(mac);
        
        // iPhones typically have stronger mobility characteristics
        if (rssi > -45 && macSuffix % 2 == 0) {
            return "Apple iPhone";
        } else if (rssi > -50) {
            return "Apple MacBook";
        } else {
            return "Apple Device";
        }
    }

    String guessBySignalCharacteristics(const String& mac, int rssi) const {
        // Guess based on signal characteristics
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
        // Get last few digits of MAC address as feature
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
        
        if (n <= 0) return;

        Serial.println("\n=== Detailed Device Analysis ===");
        
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
            Serial.println(":");
            Serial.print("  MAC: ");
            Serial.println(mac);
            Serial.print("  RSSI: ");
            Serial.print(rssi);
            Serial.println(" dBm");
            Serial.print("  SSID: ");
            Serial.println(ssid);
            Serial.print("  Type: ");
            Serial.println(deviceType);
            Serial.print("  Vendor: ");
            Serial.println(typeDetector.getVendorFromMAC(mac));
            
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
            
            Serial.println("  ---");
        }

        Serial.println("=== Summary ===");
        Serial.print("Computers/Laptops: ");
        Serial.println(computerCount);
        Serial.print("Phones: ");
        Serial.println(phoneCount);
        Serial.print("Routers: ");
        Serial.println(routerCount);
        Serial.print("Unknown: ");
        Serial.println(unknownCount);
        Serial.print("Estimated People: ");
        Serial.println(estimatePeople(phoneCount, computerCount));
        
        WiFi.scanDelete();
    }

    String classifyDevice(const String& mac, int rssi, const String& ssid) {
        // Rule 1: Check MAC address vendor
        String vendor = typeDetector.getVendorFromMAC(mac);
        
        // Rule 2: Check SSID characteristics
        if (isLikelyRouter(ssid)) {
            return "WiFi Router/Access Point";
        }
        
        // Rule 3: Based on signal strength
        if (rssi > -35) {
            return "Likely Computer (Very Strong Signal)";
        }
        
        // Rule 4: Based on vendor-specific rules
        if (vendor == "Apple") {
            return typeDetector.guessAppleDeviceType(mac, rssi);
        }
        
        // Rule 5: Mobile devices typically have more signal variation
        if (rssi > -60 && rssi < -40) {
            return "Likely Mobile Phone";
        }
        
        return "Unknown Device";
    }

    bool isLikelyRouter(const String& ssid) const {
        // Common router SSID patterns
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
        
        // Check for common router characteristics
        if (ssid.length() == 0 || ssid == "hidden") {
            return true;
        }
        
        return false;
    }

    int estimatePeople(int phoneCount, int computerCount) const {
        // Improved people estimation algorithm
        // Assumption: Each person typically has 1 phone, possibly 1 computer
        int estimatedFromPhones = phoneCount;
        int estimatedFromComputers = computerCount; 
        
        return (estimatedFromPhones > estimatedFromComputers) ? estimatedFromPhones : estimatedFromComputers;
    }
};

class BehavioralAnalyzer {
private:
    struct SignalBehavior {
        String mac;
        int minRSSI;
        int maxRSSI;
        int variance;
        bool isMobile;
    };

    std::map<String, SignalBehavior> behaviorHistory;

public:
    void trackDeviceBehavior(const String& mac, int rssi) {
        if (behaviorHistory.find(mac) == behaviorHistory.end()) {
            // New device
            behaviorHistory[mac] = {mac, rssi, rssi, 0, false};
        } else {
            // Update existing device
            auto& behavior = behaviorHistory[mac];
            behavior.minRSSI = (behavior.minRSSI < rssi) ? behavior.minRSSI : rssi;
            behavior.maxRSSI = (behavior.maxRSSI > rssi) ? behavior.maxRSSI : rssi;
            behavior.variance = behavior.maxRSSI - behavior.minRSSI;
            
            // If signal varies greatly, likely a mobile device
            behavior.isMobile = (behavior.variance > 15);
        }
    }

    String analyzeMobility(const String& mac) const {
        auto it = behaviorHistory.find(mac);
        if (it == behaviorHistory.end()) {
            return "Unknown (No history)";
        }
        
        const auto& behavior = it->second;
        
        if (behavior.isMobile) {
            return "Mobile Device (High signal variance)";
        } else if (behavior.variance > 5) {
            return "Likely Portable Device";
        } else {
            return "Likely Stationary Device";
        }
    }
};

EnhancedPeopleCounter peopleCounter;
BehavioralAnalyzer behaviorAnalyzer;

void setup() {
    Serial.begin(115200);
    delay(3000);
    
    WiFi.mode(WIFI_STA);
    WiFi.disconnect();
    delay(1000);
    
    Serial.println("Device Type Detection System Ready");
}

void loop() {
    Serial.print("\n");
    Serial.print(millis());
    Serial.println(": Scanning...");
    
    peopleCounter.analyzeDevices();
    
    delay(15000); // 15 second scan interval
}
