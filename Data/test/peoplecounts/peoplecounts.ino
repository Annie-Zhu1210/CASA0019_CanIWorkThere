#include <WiFi.h>
#include <vector>
#include <map>

class DeviceTypeDetector {
private:
    // MAC address vendor prefix database (partial)
    std::map<String, String> macVendors = {
        {"A8:5B:F7", " HP"},        
        {"24:E1:24", " MilesightRouter"},        // Xiamen Milesight IoT Co, Ltd Router
        {"8C:85:80", "AnkerEufy "},     // smart-home devices
        {"DC:A6:32", "Raspberry Pi"}, // Raspberry Pi
        {"B8:27:EB", "Raspberry Pi"},
        {"00:50:F1", "MaxLinear"},         // the Ethernet interface-dock/gateway/router
        {"00:1A:11", "Google"},       // Google devices
        {"38:F9:D3", "Apple"},
        {"44:07:0B", "Googlei"},       
        {"AC:BC:32", "Apple"},      // Apple
        {"34:BB:1F", "BlackBerry"},    // BlackBerry
        {"98:0C:82", "Samsung"},         // Samsung
        {"C0:EE:FB", "OnePlus"},      // OnePlus
        {"14:F6:5A", "Xiaomi"},       // Xiaomi
        {"08:EE:8B", "Samsung"},
        {"48:AD:08", "HUAWEI"},
        {"2C:AB:00", "HUAWEI"},
        {"00:E0:FC", "HUAWEI"},
        {"24:DF:6A", "HUAWEI"},
        {"00:9A:CD", "HUAWEI"},
        {"80:38:BC", "HUAWEI"},
        {"D4:40:F0", "HUAWEI"},
        {"64:A6:51", "HUAWEI"},
        {"E8:CD:2D", "HUAWEI"},
        {"AC:E2:15", "HUAWEI"},
        {"EC:23:3D", "HUAWEI"},
        {"78:F5:FD", "HUAWEI"},
        {"80:B6:86", "HUAWEI"},
        {"10:C6:1F", "HUAWEI"},
        {"88:53:D4", "HUAWEI"},
        {"0C:37:DC", "HUAWEI"},
        {"BC:76:70", "HUAWEI"},
        {"24:DB:AC", "HUAWEI"},
        {"0C:45:BA", "HUAWEI"},
        {"CC:A2:23", "HUAWEI"},
        {"E8:08:8B", "HUAWEI"},
        {"60:E7:01", "HUAWEI"},
        {"AC:85:3D", "HUAWEI"},
        {"74:88:2A", "HUAWEI"},
        {"78:D7:52", "HUAWEI"},
        {"E0:24:7F", "HUAWEI"},
        {"00:46:4B", "HUAWEI"},
        {"70:7B:E8", "HUAWEI"},
        {"54:89:98", "HUAWEI"},
        {"08:19:A6", "HUAWEI"},
        {"3C:F8:08", "HUAWEI"},
        {"B4:15:13", "HUAWEI"},
        {"28:31:52", "HUAWEI"},
        {"DC:D2:FC", "HUAWEI"},
        {"28:5F:DB", "HUAWEI"},
        {"40:4D:8E", "HUAWEI"},
        {"78:1D:BA", "HUAWEI"},
        {"00:1E:10", "HUAWEI"},
        {"D0:3E:5C", "HUAWEI"},
        {"F8:98:B9", "HUAWEI"},
        {"2C:CF:58", "HUAWEI"},
        {"E4:C2:D1", "HUAWEI"},
        {"00:E0:18", "ASUS"},
        {"00:0C:6E", "ASUS"},
        {"00:1B:FC", "ASUS"},
        {"00:1E:8C", "ASUS"},
        {"00:15:F2", "ASUS"},
        {"00:23:54", "ASUS"},
        {"00:1F:C6", "ASUS"},
        {"F8:32:E4", "ASUS"},
        {"38:F2:3E", "Microsoft"},
        {"74:A7:8E", "ZTE"},
        {"84:74:2A", "ZTE"},
        {"68:1A:B2", "ZTE"},
        {"80:7A:BF", "HTC"},
        {"90:E7:C4", "HTC"},
        {"7C:61:93", "HTC"},
        {"3C:D9:2B", "HP"},
        {"9C:8E:99", "HP"},
        {"B4:99:BA", "HP"},
        {"1C:C1:DE", "HP"},
        {"F4:CE:46", "HP"},
        {"00:1C:C4", "HP"},
        {"00:25:B3", "HP"},
        {"00:18:71", "HP"},
        {"00:0B:CD", "HP"},
        {"00:0E:7F", "HP"},
        {"00:0F:20", "HP"},
        {"00:11:0A", "HP"},
        {"00:13:21", "HP"},
        {"00:16:35", "HP"},
        {"00:17:A4", "HP"},
        {"00:08:02", "HP"},
        {"00:08:83", "HP"},
        {"C4:34:6B", "HP"},
        {"8C:DC:D4", "HP"},
        {"34:64:A9", "HP"},
        {"D4:C9:EF", "HP"},
        {"A4:5D:36", "HP"},
        {"A0:D3:C1", "HP"},
        {"40:A8:F0", "HP"},
        {"6C:3B:E5", "HP"},
        {"08:2E:5F", "HP"},
        {"28:92:4A", "HP"},
        {"10:60:4B", "HP"},
        {"30:8D:99", "HP"},
        {"00:30:C1", "HP"},
        {"FC:3F:DB", "HP"},
        {"00:03:47", "Intel"},
        {"00:11:75", "Intel"},
        {"00:13:E8", "Intel"},
        {"00:13:02", "Intel"},
        {"E4:F8:9C", "Intel"},
        {"A4:02:B9", "Intel"},
        {"4C:34:88", "Intel"},
        {"E0:05:C5", "TP-Link"},
        {"A0:F3:C1", "TP-Link"},
        {"8C:21:0A", "TP-Link"},
        {"EC:17:2F", "TP-Link"},
        {"EC:88:8F", "TP-Link"},
        {"14:CF:92", "TP-Link"},
        {"64:56:01", "TP-Link"},
        {"14:CC:20", "TP-Link"},
        {"BC:46:99", "TP-Link"},
        {"3C:5A:B4", "Google"},
        {"00:1A:11", "Google"},
        {"00:50:BA", "D-Link"},
        {"00:17:9A", "D-Link"},
        {"1C:BD:B9", "D-Link"},
        {"90:94:E4", "D-Link"},
        {"28:10:7B", "D-Link"},
        {"1C:7E:E5", "D-Link"},
        {"C4:A8:1D", "D-Link"},
        {"00:12:17", "Cisco-Linksys"},
        {"00:0C:41", "Cisco-Linksys"},
        {"00:0F:66", "Cisco-Linksys"},
        {"CC:46:D6", "Cisco"},
        {"58:AC:78", "Cisco"},
        {"00:10:7B", "Cisco"},
        {"00:90:6D", "Cisco"},
        {"00:90:BF", "Cisco"},
        {"00:50:80", "Cisco"},
        {"F4:CF:E2", "Cisco"},
        {"50:1C:BF", "Cisco"},
        {"88:F0:31", "Cisco"},
        {"50:87:89", "Cisco"},
        {"38:1C:1A", "Cisco"},
        {"F4:0F:1B", "Cisco"},
        {"BC:67:1C", "Cisco"},
        {"A0:EC:F9", "Cisco"},
        {"D4:6D:50", "Cisco"},
        {"1C:E8:5D", "Cisco"},
        {"C4:72:95", "Cisco"},
        {"A0:55:4F", "Cisco"},
        {"84:B8:02", "Cisco"},
        {"BC:C4:93", "Cisco"},
        {"F2:29:29", "Cisco"},
        {"EC:E1:A9", "Cisco"},
        {"7C:69:F6", "Cisco"},
        {"C0:8C:60", "Cisco"},
        {"C0:25:5C", "Cisco"},
        {"88:5A:92", "Cisco"},
        {"E4:C7:22", "Cisco"},
        {"C0:7B:BC", "Cisco"},
        {"00:90:F2", "Cisco"},
        {"00:17:3B", "Cisco"},
        {"00:40:0B", "Cisco"},
        {"00:60:09", "Cisco"},
        {"00:60:47", "Cisco"},
        {"00:06:C1", "Cisco"},
        {"00:E0:14", "Cisco"},
        {"00:E0:1E", "Cisco"},
        {"AC:F2:C5", "Cisco"},
        {"00:10:FF", "Cisco"},
        {"34:BD:C8", "Cisco"},
        {"54:A2:74", "Cisco"},
        {"58:97:BD", "Cisco"},
        {"04:6C:9D", "Cisco"},
        {"78:D6:B2", "Toshiba"},
        {"00:0D:0B", "Buffalo"},
        {"00:07:40", "Buffalo"},
        {"00:24:A5", "Buffalo"},
        {"DC:FB:02", "Buffalo"},
        {"18:62:2C", "Sagemcom"},
        {"7C:03:D8", "Sagemcom"},
        {"E8:F1:B0", "Sagemcom"},
        {"34:8A:AE", "Sagemcom"},
        {"00:21:E8", "Murata"},
        {"00:60:57", "Murata"},
        {"00:07:D8", "Hitron"},
        {"90:7F:61", "Chicony"},
        {"40:9F:87", "Jide"},
        {"D8:3C:69", "Tinno"},
        {"74:AC:5F", "Qiku"},
        {"00:6B:8E", "Shanghai Feixun"},
        {"00:03:DD", "Comark"},
        {"3C:8C:F8", "TRENDnet"},
        {"44:E0:8E", "Cisco SPVTG"},
        {"18:59:33", "Cisco SPVTG"},
        {"E4:48:C7", "Cisco SPVTG"},
        {"24:76:7D", "Cisco SPVTG"},
        {"2C:AB:A4", "Cisco SPVTG"},
        {"00:19:47", "Cisco SPVTG"},
        {"00:22:CE", "Cisco SPVTG"},
        {"F4:4B:2A", "Cisco SPVTG"}
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
