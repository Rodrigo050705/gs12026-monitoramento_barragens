import time
import json
import math
import random
import paho.mqtt.client as mqtt
from datetime import datetime, timezone

# --- CONFIGURAÇÕES ---
BROKER = "localhost"
PORT = 1883
TOPICO_PIEZOMETRO = "barragens/sensor/piezometro"
TOPICO_NIVEL = "barragens/sensor/nivel"

# Inicializa o cliente MQTT
client = mqtt.Client(callback_api_version=mqtt.CallbackAPIVersion.VERSION2)

try:
    client.connect(BROKER, PORT, 60)
    print(f"✅ Conectado ao Broker MQTT em {BROKER}:{PORT}")
except Exception as e:
    print(f"❌ Erro ao conectar ao Broker: {e}")
    print("Certifique-se de que o container do Docker (mosquitto) está rodando.")
    exit(1)

print("🚀 Simulador do Dam Monitor iniciado. Pressione Ctrl+C para parar.\n")

# Variável de tempo para criar a onda suave (senoide)
contador_ondas = 0
ciclo_atual = 0  # <--- Nova variável para contar os passos

while True:
    try:
        # 1. GERAÇÃO DE DADOS SIMULADOS (Com comportamento real)
        contador_ondas += 0.1
        ciclo_atual += 1  # Incrementa 1 a cada 5 segundos
        timestamp_atual = datetime.now(timezone.utc).isoformat()
        
        # --- Simulação do Piezômetro ---
        # Base de 140 kPa + variação suave + ruído aleatório menor
        pressao_base = 140.0 + (math.sin(contador) * 5) + random.uniform(-0.5, 0.5)
        
        # --- Simulação do Sensor de Nível ---
        # Base de 22 metros + variação suave
        nivel_base = 22.0 + (math.cos(contador * 0.5) * 2) + random.uniform(-0.1, 0.1)
        
        # --- INJEÇÃO DE ANOMALIA (Gatilho de Teste) ---
        # A cada ~50 ciclos, vamos forçar um valor crítico para testar os alertas do Matheus
        forçar_alerta = random.randint(1, 50) == 25
        if forçar_alerta:
            print("\n🚨 [SIMULADOR] Injetando uma anomalia crítica para teste de estresse!")
            pressao_base += 45.0  # Vai saltar para quase 180 kPa (Crítico)
            nivel_base += 8.0     # Vai saltar para 30 metros (Crítico)

        # 2. MONTAGEM DOS PAYLOADS JSON (Conforme o contrato que alinhamos)
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

        # 3. ENVIO DOS DADOS PARA OS TÓPICOS MQTT
        client.publish(TOPICO_PIEZOMETRO, json.dumps(payload_piezometro))
        client.publish(TOPICO_NIVEL, json.dumps(payload_nivel))

        # Logs no console para o Rodrigo acompanhar
        print(f"[{datetime.now().strftime('%H:%M:%S')}] 📡 Dados enviados:")
        print(f"  -> Piezômetro: {payload_piezometro['leitura']['pressao_kPa']} kPa")
        print(f"  -> Nível: {payload_nivel['leitura']['altura_metros']} m")
        if forçar_alerta:
            print("  ⚠️ Alerta enviado nas leituras acima!")
        print("-" * 40)

        # Aguarda 5 segundos antes da próxima leitura
        time.sleep(5)

    except KeyboardInterrupt:
        print("\n👋 Simulador encerrado pelo usuário.")
        client.disconnect()
        break
