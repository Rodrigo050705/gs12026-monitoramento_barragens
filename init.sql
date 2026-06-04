-- ============================================
-- GLOBAL SOLUTION 2026
-- SISTEMA DE MONITORAMENTO DE BARRAGENS
-- ============================================

-- ============================================
-- TABELA USUARIOS
-- ============================================

CREATE TABLE usuarios (
    id SERIAL PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    email VARCHAR(150) UNIQUE NOT NULL,
    senha_hash VARCHAR(255) NOT NULL,
    role VARCHAR(20) NOT NULL
);

-- ============================================
-- TABELA BARRAGENS
-- ============================================

CREATE TABLE barragens (
    id SERIAL PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    localizacao VARCHAR(200) NOT NULL,
    nivel_critico_metros NUMERIC(10,2) NOT NULL
);

-- ============================================
-- TABELA SENSORES
-- ============================================

CREATE TABLE sensores (
    id SERIAL PRIMARY KEY,
    codigo_identificador VARCHAR(50) UNIQUE NOT NULL,
    tipo VARCHAR(30) NOT NULL,
    limite_alerta NUMERIC(10,2) NOT NULL,
    barragem_id INTEGER NOT NULL,

    CONSTRAINT fk_sensor_barragem
        FOREIGN KEY (barragem_id)
        REFERENCES barragens(id)
);

-- ============================================
-- TABELA LEITURAS
-- ============================================

CREATE TABLE leituras (
    id BIGSERIAL PRIMARY KEY,
    sensor_id INTEGER NOT NULL,
    timestamp TIMESTAMP NOT NULL,
    valor_leitura NUMERIC(10,2) NOT NULL,

    CONSTRAINT fk_leitura_sensor
        FOREIGN KEY (sensor_id)
        REFERENCES sensores(id)
);

-- ============================================
-- TABELA ALERTAS
-- ============================================

CREATE TABLE alertas (
    id SERIAL PRIMARY KEY,
    sensor_id INTEGER NOT NULL,
    mensagem VARCHAR(255) NOT NULL,
    timestamp TIMESTAMP NOT NULL,
    status VARCHAR(20) NOT NULL,

    CONSTRAINT fk_alerta_sensor
        FOREIGN KEY (sensor_id)
        REFERENCES sensores(id)
);

-- ============================================
-- INSERTS DE TESTE
-- ============================================

INSERT INTO usuarios
(nome, email, senha_hash, role)
VALUES
('Administrador', 'admin@barragens.com', 'hash123', 'ADMIN'),
('Operador João', 'joao@barragens.com', 'hash456', 'OPERADOR');

INSERT INTO barragens
(nome, localizacao, nivel_critico_metros)
VALUES
('Barragem Sul', 'Minas Gerais', 18.00),
('Barragem Norte', 'Pará', 20.00);

INSERT INTO sensores
(codigo_identificador, tipo, limite_alerta, barragem_id)
VALUES
('PZ001', 'PIEZOMETRO', 15.00, 1),
('NV001', 'NIVEL_AGUA', 18.00, 1),
('PZ002', 'PIEZOMETRO', 16.00, 2);

INSERT INTO leituras
(sensor_id, timestamp, valor_leitura)
VALUES
(1, CURRENT_TIMESTAMP, 12.50),
(1, CURRENT_TIMESTAMP, 13.20),
(2, CURRENT_TIMESTAMP, 17.40),
(3, CURRENT_TIMESTAMP, 15.90);

INSERT INTO alertas
(sensor_id, mensagem, timestamp, status)
VALUES
(
    2,
    'Nível da água próximo do limite crítico',
    CURRENT_TIMESTAMP,
    'ATIVO'
),
(
    3,
    'Pressão acima do limite configurado',
    CURRENT_TIMESTAMP,
    'ATIVO'
);

-- ============================================
-- CONSULTAS DE DEMONSTRAÇÃO
-- ============================================

-- 1. Listar usuários

SELECT *
FROM usuarios;

-- 2. Listar barragens

SELECT *
FROM barragens;

-- 3. Listar sensores com suas barragens

SELECT
    s.id,
    s.codigo_identificador,
    s.tipo,
    s.limite_alerta,
    b.nome AS barragem
FROM sensores s
INNER JOIN barragens b
    ON s.barragem_id = b.id;

-- 4. Exibir leituras dos sensores

SELECT
    l.id,
    s.codigo_identificador,
    s.tipo,
    l.valor_leitura,
    l.timestamp
FROM leituras l
INNER JOIN sensores s
    ON l.sensor_id = s.id
ORDER BY l.timestamp DESC;

-- 5. Mostrar alertas ativos

SELECT
    a.id,
    s.codigo_identificador,
    a.mensagem,
    a.status,
    a.timestamp
FROM alertas a
INNER JOIN sensores s
    ON a.sensor_id = s.id
WHERE a.status = 'ATIVO';

-- 6. Quantidade de sensores por barragem

SELECT
    b.nome,
    COUNT(s.id) AS quantidade_sensores
FROM barragens b
LEFT JOIN sensores s
    ON b.id = s.barragem_id
GROUP BY b.nome;

-- 7. Quantidade de alertas por sensor

SELECT
    s.codigo_identificador,
    COUNT(a.id) AS total_alertas
FROM sensores s
LEFT JOIN alertas a
    ON s.id = a.sensor_id
GROUP BY s.codigo_identificador;

-- 8. Média das leituras por sensor

SELECT
    s.codigo_identificador,
    AVG(l.valor_leitura) AS media_leituras
FROM sensores s
INNER JOIN leituras l
    ON s.id = l.sensor_id
GROUP BY s.codigo_identificador;

-- 9. Sensores que ultrapassaram o limite de alerta

SELECT
    s.codigo_identificador,
    l.valor_leitura,
    s.limite_alerta
FROM sensores s
INNER JOIN leituras l
    ON s.id = l.sensor_id
WHERE l.valor_leitura > s.limite_alerta;
