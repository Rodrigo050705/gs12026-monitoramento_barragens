FROM python:3.13-alpine

WORKDIR /app

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

COPY simulator.py .

ENV PYTHONUNBUFFERED=1
ENV MQTT_BROKER=mqtt-broker
ENV MQTT_PORT=1883

CMD ["python", "-u", "simulator.py"]
