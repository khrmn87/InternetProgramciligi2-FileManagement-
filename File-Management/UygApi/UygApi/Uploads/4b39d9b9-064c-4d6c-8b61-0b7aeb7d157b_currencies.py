import requests
import json
from datetime import datetime, timezone
import hmac
import base64

class OKXClient:
    def __init__(self, api_key, api_secret, passphrase):
        self.base_url = "https://www.okx.com"
        self.api_key = api_key
        self.api_secret = api_secret
        self.passphrase = passphrase

    def _get_timestamp(self):
        return datetime.now(timezone.utc).isoformat()[:-9] + 'Z'

    def _generate_signature(self, timestamp, method, request_path, params=None):
        if params:
            request_path = request_path + '?' + '&'.join([f"{k}={v}" for k, v in sorted(params.items())])
        message = timestamp + method + request_path
        mac = hmac.new(
            bytes(self.api_secret, encoding='utf8'),
            bytes(message, encoding='utf-8'),
            digestmod='sha256'
        )
        return base64.b64encode(mac.digest()).decode()

    def get_currency(self, currency_symbol):
        """
        Belirli bir kripto para biriminin bilgilerini getirir
        
        Args:
            currency_symbol (str): Para birimi sembolü (örn: "BTC", "ETH")
        """
        endpoint = "/api/v5/asset/currencies"
        method = "GET"
        params = {"ccy": currency_symbol}
        
        timestamp = self._get_timestamp()
        signature = self._generate_signature(timestamp, method, endpoint, params)
        
        headers = {
            'OK-ACCESS-KEY': self.api_key,
            'OK-ACCESS-SIGN': signature,
            'OK-ACCESS-TIMESTAMP': timestamp,
            'OK-ACCESS-PASSPHRASE': self.passphrase,
            'Content-Type': 'application/json'
        }

        try:
            response = requests.get(
                self.base_url + endpoint,
                headers=headers,
                params=params
            )
            
            if response.status_code == 200:
                data = response.json()
                print(json.dumps(data, indent=2))
                return data
            else:
                print(f"Hata: API yanıt kodu {response.status_code}")
                print(f"Yanıt içeriği: {response.text}")
                return None
                
        except Exception as e:
            print(f"Bir hata oluştu: {str(e)}")
            return None

# Kullanım örneği
if __name__ == "__main__":
    API_KEY = "5801fab8-4891-4694-bc9e-c6e407ec94c4"
    API_SECRET = "BAD3D2AC75108D471CBA51B33B2ADCAB"
    PASSPHRASE = "AliBayirli05/"
    
    client = OKXClient(API_KEY, API_SECRET, PASSPHRASE)
    
    # Bitcoin bilgilerini al
    btc_info = client.get_currency("BTC")
    
    # veya
    # Ethereum bilgilerini al
    # eth_info = client.get_currency("ETH")