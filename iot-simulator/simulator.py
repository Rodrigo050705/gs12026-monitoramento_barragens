import time
import json
import math
import os
import random
import paho.mqtt.client as mqtt
from datetime import datetime, timezone

# --- CONFIGURAÇÕES ---
BROKER = os.getenv("MQTT_BROKER", "localhost")
PORT = int(os.getenv("MQTT_PORT", "1883"))
TOPICO_PIEZOMETRO = "barragens/sensor/piezometro"
TOPICO_NIVEL = "barragens/sensor/nivel"

# Inicializa o cliente MQTT
client = mqtt.Client(callback_api_version=mqtt.CallbackAPIVersion.VERSION2)

try:
    client.connect(BROKER, PORT, 60)
    client.loop_start()
    print(f"✅ Conectado ao Broker MQTT em {BROKER}:{PORT}")
except Exception as e:
    print(f"❌ Erro ao conectar ao Broker: {e}")
    print("Certifique-se de que o container do Docker (mosquitto) está rodando.")
    exit(1)

def enviar_payload(topico, payload):
    result = client.publish(topico, json.dumps(payload))
    if result.rc != mqtt.MQTT_ERR_SUCCESS:
        print(f"⚠️ Falha ao publicar no tópico {topico}: código {result.rc}")

print("🚀 Simulador do Dam Monitor iniciado. Pressione Ctrl+C para parar.\n")

# Variável para contar ciclos
contador_ondas = 0
ciclo_atual = 0  # <--- Nova variável para contar os passos

while True:
    try:
        contador_ondas += 0.1
        ciclo_atual += 1  # Incrementa 1 a cada 5 segundos
        timestamp_atual = datetime.now(timezone.utc).isoformat()
        
        # --- Simulação normal ---
        pressao_base = 140.0 + (math.sin(contador_ondas) * 5) + random.uniform(-0.5, 0.5)
        nivel_base = 22.0 + (math.cos(contador_ondas * 0.5) * 2) + random.uniform(-0.1, 0.1)
        
        # --- NOVO GATILHO DE ANOMALIA (A cada 30 segundos / 6 ciclos) ---
        forçar_alerta = (ciclo_atual % 6 == 0)
        
        if forçar_alerta:
            print("\n🚨 [SIMULADOR] Injetando uma anomalia crítica automática (Ciclo de 30s)!")
            pressao_base += 45.0  # Salta para ~188 kPa (Crítico)
            nivel_base += 8.0     # Salta para ~30 metros (Crítico)

        # --- MONTAGEM DOS PAYLOADS JSON ---
        payload_piezometro = {
            "sensor_id": "PIEZ-BRG01-04",
            "timestamp": timestamp_atual,
            "tipo": "piezometro",
            "leitura": {
                "pressao_kPa": round(pressao_base, 2)
            },
            "status_bateria": random.randint(90, 95)
        }

        payload_nivel = {
            "sensor_id": "NIVL-BRG01-01",
            "timestamp": timestamp_atual,
            "tipo": "nivel",
            "leitura": {
                "altura_metros": round(nivel_base, 2)
            },
            "status_bateria": random.randint(85, 90)
        }

        # --- ENVIO DOS DADOS ---
        enviar_payload(TOPICO_PIEZOMETRO, payload_piezometro)
        enviar_payload(TOPICO_NIVEL, payload_nivel)

        # Logs no console
        print(f"[{datetime.now().strftime('%H:%M:%S')}] 📡 Dado enviado (Ciclo #{ciclo_atual}):")
        print(f"  -> Piezômetro: {payload_piezometro['leitura']['pressao_kPa']} kPa")
        print(f"  -> Nível: {payload_nivel['leitura']['altura_metros']} m")
        if forçar_alerta:
            print("  ⚠️ VALORES CRÍTICOS ENVIADOS!")
        print("-" * 40)

        time.sleep(5)

    except KeyboardInterrupt:
        print("\n👋 Simulador encerrado pelo usuário.")
        client.loop_stop()
        client.disconnect()
        break
