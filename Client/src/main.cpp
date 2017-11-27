#include <DNSServer.h>           

#include <WiFiManager.h>         
#include <ArduinoJson.h>
WiFiUDP udpClient;
WiFiClient tcpClient;

enum class ClientType
{
        None = 0,
        Mcu,
        Phone,
        Pc
};

enum class MessageType
{
    ReceiveClient = 0,
    SendMessage,
    DeactivateAlarm
};

class NetworkData
 {
public:
    NetworkData()
    {

    }
   explicit NetworkData(const JsonObject& root)
    {
        clientType = static_cast<ClientType>(root["ClientType"].as<int>());
        Message = root["Message"].as<char*>();
        Type = static_cast<MessageType>((root["MessageType"].as<int>()));
    }
    MessageType Type;
    String Message;
    ClientType clientType = ClientType::None;
    const String& GetMessage() const
    {
        return Message;
    }
};
constexpr int readPin = 5;
constexpr int builtin = LED_BUILTIN;

void setup()
{
    Serial.begin(115200);
    WiFi.hostname("ESP8266");
    WiFiManager wifiManager;
    wifiManager.autoConnect("ESP8266");
    udpClient.begin(10500);
    pinMode(readPin, INPUT);
    pinMode(builtin, OUTPUT);
}

void loop()
{
    if(!tcpClient.connected())
    {
        auto length = udpClient.parsePacket();
        if(length)
        {
            Serial.println("Conectando...");
            String jsonString = udpClient.readString();
            DynamicJsonBuffer jsonBuffer;
            JsonObject& root = jsonBuffer.parse(jsonString);
            auto data = NetworkData(root);
            IPAddress address;
            address.fromString(data.Message);
            Serial.println(String("Direccion: ") + data.Message);
            tcpClient.connect(address, 10501);
        }
    }
    else
    {
        
        if(digitalRead(readPin) == LOW)
        {
            digitalWrite(builtin, LOW);
            DynamicJsonBuffer jsonBuffer;
            JsonObject& root = jsonBuffer.createObject();
            root["ClientType"] = static_cast<int>(ClientType::Mcu);
            root["Message"] = "activate alarm";
            root["MessageType"] = static_cast<int>(MessageType::SendMessage);
            String message = "";
            root.printTo(message);
            tcpClient.write(message.c_str());
        }
        else
        {
            digitalWrite(builtin, HIGH);
        }
        delay(50);
    }
}