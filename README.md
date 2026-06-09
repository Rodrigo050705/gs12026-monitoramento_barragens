# 🌊 Dam Monitor - Sistema de Monitoramento de Barragens

O **Dam Monitor** é um ecossistema distribuído de alta disponibilidade desenvolvido para a **Global Solution 2026**. O objetivo principal da solução é realizar a segurança preditiva e o monitoramento telemétrico em tempo real de poropressão e nível de água em barragens de rejeitos, utilizando uma arquitetura moderna orientada a eventos e microsserviços.

---

## 🏗️ Arquitetura do Sistema

O projeto é composto por quatro camadas principais operando de forma desacoplada e resiliente:

1. **Camada IoT (Simulação):** Script em Python (`simulator.py`) que mimetiza o comportamento de sensores de campo, transmitindo payloads estruturados em formato JSON via protocolo MQTT.
2. **Mensageria (Broker):** Servidor Eclipse Mosquitto atuando como o ponto central de recepção da telemetria de campo.
3. **Back-end (Inteligência):** API desenvolvida em **C# (.NET 8)** operando como um Worker assíncrono de alto desempenho. É responsável pela higienização de strings, validação relacional e disparos automatizados de alertas baseados em *thresholds* estáticos.
4. **Banco de Dados (Persistência):** Banco de dados relacional **PostgreSQL** modelado de forma otimizada para alta volumetria (utilizando o tipo `BIGSERIAL` para o histórico de leituras).

---

## 🚀 Pré-requisitos

Antes de iniciar, certifique-se de ter instalado em sua máquina:
* [Docker Desktop](https://www.docker.com/products/docker-desktop/) (com Docker Compose ativo)
* [Python 3.10+](https://www.python.org/downloads/)
* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (opcional, caso queira rodar a API fora do container)
* vs code (front)
* visual studio (C#)
*  Node.js para react no front
* verificar instalação da biblioteca de comunicação MQTT: pip install paho-mqtt
---

## 🛠️ Passo a Passo para Execução

### 1. Clonar o Repositório e Atualizar o Código
git clone do repositório

### 2. Rodar docker
acessar pasta do repositório dentro do terminal do docker

executar comando: docker compose up -d

### 3. Executar back local
acessar pasta do back no visual studio

executar solução "DamMonitor.sln"

para visualizar documentação da api: vá ao link indicado no terminal e acrescente "/scalar". Ou clique no link do docker "dam_monitor_api"

### 4. Receber dados
usar mqttx e usar caminho: barragens/sensor/#.

### 5. Gerar e envio de dados simulados
acessar pasta iot-simulator no terminal do computador

executar: python simulator.py

### 6. Front
abrir projeto front em vscode e executar comandos: npm-install e npm start

Acessar link 
