

-- Расширения (idempotent)
CREATE EXTENSION IF NOT EXISTS postgis;
CREATE EXTENSION IF NOT EXISTS hstore;
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- Namespace UUID v5 (фиксированный, стандартный DNS namespace).
-- Используется инлайном в каждом uuid_generate_v5(...) вызове.

-- =====================================================================
-- Helper: выбор имени на нужном языке.
--   1. name:ru (русский)        — предпочитаемый
--   2. name:be (беларусский)    — fallback если русского нет
--   3. name (дефолтный OSM name) — может быть любым языком, но лучше чем ничего
--   4. '<без названия>'          — последний рубеж
-- Все теги лежат в hstore p.tags (osm2pgsql --hstore).
-- =====================================================================
CREATE OR REPLACE FUNCTION addr_pick_name(tags hstore, default_name text)
RETURNS text
LANGUAGE sql
IMMUTABLE
AS $$
    SELECT COALESCE(
        NULLIF(tags->'name:ru', ''),
        NULLIF(tags->'name:be', ''),
        NULLIF(default_name, ''),
        '<без названия>'
    )
$$;

-- =====================================================================
-- Диагностика перед ETL
-- =====================================================================
\echo '=== planet_osm_* row counts ==='
SELECT 'planet_osm_point'   AS tbl, COUNT(*) FROM planet_osm_point
UNION ALL SELECT 'planet_osm_line',    COUNT(*) FROM planet_osm_line
UNION ALL SELECT 'planet_osm_polygon', COUNT(*) FROM planet_osm_polygon;

\echo '=== planet_osm_polygon columns (filter: relevant for ETL) ==='
SELECT column_name, data_type
FROM information_schema.columns
WHERE table_name = 'planet_osm_polygon'
  AND column_name IN ('osm_id','name','boundary','admin_level','place','highway','building',
                      'addr:housenumber','addr:street','addr:postcode','area','way','tags')
ORDER BY column_name;

\echo '=== planet_osm_point columns (filter) ==='
SELECT column_name, data_type
FROM information_schema.columns
WHERE table_name = 'planet_osm_point'
  AND column_name IN ('osm_id','name','place','way','tags')
ORDER BY column_name;

\echo '=== planet_osm_line columns (filter) ==='
SELECT column_name, data_type
FROM information_schema.columns
WHERE table_name = 'planet_osm_line'
  AND column_name IN ('osm_id','name','highway','area','way','tags')
ORDER BY column_name;

\echo '=== Pre-ETL counts of candidates ==='
SELECT 'regions (admin_level=4 + name)'            AS what, COUNT(*) FROM planet_osm_polygon p WHERE p.boundary = 'administrative' AND p.admin_level = '4' AND p.name IS NOT NULL
UNION ALL SELECT 'cities (place in city/town)',          COUNT(*) FROM planet_osm_point   p WHERE p.place IN ('city','town')        AND p.name IS NOT NULL
UNION ALL SELECT 'villages (place in village/hamlet)',   COUNT(*) FROM planet_osm_point   p WHERE p.place IN ('village','hamlet')   AND p.name IS NOT NULL
UNION ALL SELECT 'suburbs (place in suburb/neighb.)',    COUNT(*) FROM planet_osm_point   p WHERE p.place IN ('suburb','neighbourhood') AND p.name IS NOT NULL
UNION ALL SELECT 'streets (highway with name)',          COUNT(*) FROM planet_osm_line    l WHERE l.highway IS NOT NULL AND l.highway NOT IN ('service','footway','path','cycleway','steps','track') AND l.name IS NOT NULL
UNION ALL SELECT 'buildings (building + housenumber)',   COUNT(*) FROM planet_osm_polygon p WHERE p.building IS NOT NULL AND p."addr:housenumber" IS NOT NULL;

-- =====================================================================
-- Шаг 1. Регионы (Минская обл. и т.д.)
--   Источник: planet_osm_polygon с boundary=administrative, admin_level=4
--   parent: нет (корень иерархии)
--   geom: ST_Centroid(p.way) — нужен для поиска ближайшего родителя на шаге 2
-- =====================================================================
\echo ''
\echo '=== Шаг 1: Регионы ==='

-- osm2pgsql разбивает multipolygon-отношения на несколько строк с одинаковым
-- osm_id (по одному на кольцо). Используем две CTE:
--   region_geom  — GROUP BY + ST_Union объединяет геометрии колец в один полигон
--   region_attrs — DISTINCT ON берёт ОДНУ строку на osm_id для name
--                  (MAX() на hstore не работает — нет такого агрегата в PG)
WITH region_geom AS (
    SELECT
        p.osm_id,
        ST_Centroid(ST_Union(p.way)) AS centroid
    FROM planet_osm_polygon p
    WHERE p.boundary = 'administrative'
      AND p.admin_level = '4'
      AND p.name IS NOT NULL
    GROUP BY p.osm_id
),
region_attrs AS (
    SELECT DISTINCT ON (p.osm_id)
        p.osm_id,
        addr_pick_name(p.tags, p.name) AS name
    FROM planet_osm_polygon p
    WHERE p.boundary = 'administrative'
      AND p.admin_level = '4'
      AND p.name IS NOT NULL
    ORDER BY p.osm_id
)
INSERT INTO address_objects (id, parent_id, osm_id, osm_type, name, type, full_path, geom, created_at, updated_at)
SELECT
    uuid_generate_v5('6ba7b810-9dad-11d1-80b4-00c04fd430c8'::uuid, 'region:' || rg.osm_id::text),
    NULL,
    rg.osm_id,
    CASE WHEN rg.osm_id < 0 THEN 'relation' ELSE 'way' END,
    ra.name,
    'region',
    ra.name,
    rg.centroid,
    now(),
    now()
FROM region_geom rg
JOIN region_attrs ra ON ra.osm_id = rg.osm_id
ON CONFLICT (osm_id, osm_type) DO UPDATE SET
    name       = EXCLUDED.name,
    full_path  = EXCLUDED.full_path,
    geom       = EXCLUDED.geom,
    updated_at = now();

SELECT 'region' AS type_inserted, COUNT(*) AS total FROM address_objects WHERE type = 'region';

-- =====================================================================
-- Шаг 2. Города / деревни (Минск, Гомель, ...)
--   Источник: planet_osm_point с place=city/town/village/hamlet
--   parent: ближайший Region (LEFT JOIN — если региона нет, вставим с parent_id=NULL)
-- =====================================================================
\echo ''
\echo '=== Шаг 2: Города / деревни ==='

INSERT INTO address_objects (id, parent_id, osm_id, osm_type, name, type, full_path, geom, created_at, updated_at)
SELECT
    uuid_generate_v5('6ba7b810-9dad-11d1-80b4-00c04fd430c8'::uuid,
        CASE p.place
            WHEN 'city'    THEN 'city:'    || p.osm_id::text
            WHEN 'town'    THEN 'city:'    || p.osm_id::text
            WHEN 'village' THEN 'village:' || p.osm_id::text
            WHEN 'hamlet'  THEN 'village:' || p.osm_id::text
        END),
    r.id,
    p.osm_id,
    'node',
    addr_pick_name(p.tags, p.name),
    CASE p.place
        WHEN 'city'    THEN 'city'::text
        WHEN 'town'    THEN 'city'::text
        WHEN 'village' THEN 'village'::text
        WHEN 'hamlet'  THEN 'village'::text
    END,
    COALESCE(r.full_path || ', ', '') || addr_pick_name(p.tags, p.name),
    p.way,
    now(),
    now()
FROM planet_osm_point p
LEFT JOIN LATERAL (
    SELECT r.id, r.full_path, r.geom
    FROM address_objects r
    WHERE r.type = 'region'
    ORDER BY r.geom <-> p.way
    LIMIT 1
) r ON TRUE
WHERE p.place IN ('city', 'town', 'village', 'hamlet')
  AND p.name IS NOT NULL
ON CONFLICT (osm_id, osm_type) DO UPDATE SET
    parent_id  = EXCLUDED.parent_id,
    name       = EXCLUDED.name,
    full_path  = EXCLUDED.full_path,
    geom       = EXCLUDED.geom,
    updated_at = now();

SELECT 'city'    AS type_inserted, COUNT(*) AS total FROM address_objects WHERE type = 'city'
UNION ALL
SELECT 'village',                    COUNT(*)           FROM address_objects WHERE type = 'village';

-- =====================================================================
-- Шаг 3. Районы города (Уручье, Малиновка) и районы области (Минский р-н)
--   Источник: planet_osm_point с place=suburb/neighbourhood/county
--   parent: ближайший City/Village (LEFT JOIN)
-- =====================================================================
\echo ''
\echo '=== Шаг 3: Районы ==='

INSERT INTO address_objects (id, parent_id, osm_id, osm_type, name, type, full_path, geom, created_at, updated_at)
SELECT
    uuid_generate_v5('6ba7b810-9dad-11d1-80b4-00c04fd430c8'::uuid,
        CASE p.place
            WHEN 'suburb'         THEN 'suburb:'   || p.osm_id::text
            WHEN 'neighbourhood'  THEN 'suburb:'   || p.osm_id::text
            WHEN 'county'         THEN 'district:' || p.osm_id::text
        END),
    c.id,
    p.osm_id,
    'node',
    addr_pick_name(p.tags, p.name),
    CASE p.place
        WHEN 'suburb'        THEN 'suburb'::text
        WHEN 'neighbourhood' THEN 'suburb'::text
        WHEN 'county'        THEN 'district'::text
    END,
    COALESCE(c.full_path || ', ', '') || addr_pick_name(p.tags, p.name),
    p.way,
    now(),
    now()
FROM planet_osm_point p
LEFT JOIN LATERAL (
    SELECT c.id, c.full_path, c.geom
    FROM address_objects c
    WHERE c.type IN ('city', 'village')
    ORDER BY c.geom <-> p.way
    LIMIT 1
) c ON TRUE
WHERE p.place IN ('suburb', 'neighbourhood', 'county')
  AND p.name IS NOT NULL
ON CONFLICT (osm_id, osm_type) DO UPDATE SET
    parent_id  = EXCLUDED.parent_id,
    name       = EXCLUDED.name,
    full_path  = EXCLUDED.full_path,
    geom       = EXCLUDED.geom,
    updated_at = now();

SELECT 'suburb'   AS type_inserted, COUNT(*) AS total FROM address_objects WHERE type = 'suburb'
UNION ALL
SELECT 'district',                    COUNT(*)           FROM address_objects WHERE type = 'district';

-- =====================================================================
-- Шаг 4. Улицы / проспекты / переулки / площади
--   Источник: planet_osm_line с highway=* (исключая service/footway/path без name)
--   parent: ближайший City/Village/Suburb (LEFT JOIN)
--   area='yes' + highway → считаем площадью (square)
-- =====================================================================
\echo ''
\echo '=== Шаг 4: Улицы / площади ==='

INSERT INTO address_objects (id, parent_id, osm_id, osm_type, name, type, full_path, geom, created_at, updated_at)
SELECT
    uuid_generate_v5('6ba7b810-9dad-11d1-80b4-00c04fd430c8'::uuid,
        CASE
            WHEN l.highway IS NOT NULL AND l.area = 'yes' THEN 'square:' || l.osm_id::text
            ELSE 'street:' || l.osm_id::text
        END),
    c.id,
    l.osm_id,
    'way',
    addr_pick_name(l.tags, l.name),
    CASE
        WHEN l.highway IS NOT NULL AND l.area = 'yes' THEN 'square'::text
        ELSE 'street'::text
    END,
    COALESCE(c.full_path || ', ', '') || addr_pick_name(l.tags, l.name),
    ST_Centroid(l.way),
    now(),
    now()
FROM planet_osm_line l
LEFT JOIN LATERAL (
    SELECT c.id, c.full_path, c.geom
    FROM address_objects c
    WHERE c.type IN ('city', 'village', 'suburb')
    ORDER BY c.geom <-> ST_Centroid(l.way)
    LIMIT 1
) c ON TRUE
WHERE l.highway IS NOT NULL
  AND l.highway NOT IN ('service', 'footway', 'path', 'cycleway', 'steps', 'track')
  AND l.name IS NOT NULL
ON CONFLICT (osm_id, osm_type) DO UPDATE SET
    parent_id  = EXCLUDED.parent_id,
    name       = EXCLUDED.name,
    full_path  = EXCLUDED.full_path,
    geom       = EXCLUDED.geom,
    updated_at = now();

SELECT 'street' AS type_inserted, COUNT(*) AS total FROM address_objects WHERE type = 'street'
UNION ALL
SELECT 'square',                    COUNT(*)           FROM address_objects WHERE type = 'square';

-- =====================================================================
-- Шаг 5. Здания (дома)
--   Источник: planet_osm_polygon с building=* + addr:housenumber
--   parent: Street (по addr:street или ближайшая) или City (LEFT JOIN)
--   name = addr:housenumber ("27", "27А", "27/2")
-- =====================================================================
\echo ''
\echo '=== Шаг 5: Здания ==='

-- CTE объединяет части multipolygon-зданий (один osm_id → несколько строк в
-- planet_osm_polygon) в один объект:
--   building_geom  — GROUP BY + ST_Union для геометрии
--   building_attrs — DISTINCT ON берёт ОДНУ строку на osm_id для housenumber/street
-- Затем LATERAL JOIN ищет ближайших родителей по объединённому центроиду.
WITH building_geom AS (
    SELECT
        p.osm_id,
        ST_Centroid(ST_Union(p.way)) AS centroid
    FROM planet_osm_polygon p
    WHERE p.building IS NOT NULL
      AND p."addr:housenumber" IS NOT NULL
      AND NULLIF(p."addr:housenumber", '') IS NOT NULL
    GROUP BY p.osm_id
),
building_attrs AS (
    SELECT DISTINCT ON (p.osm_id)
        p.osm_id,
        p."addr:housenumber" AS housenumber,
        -- addr:street НЕ является dedicated-колонкой в planet_osm_polygon
        -- (в default.style osm2pgsql он не объявлен). С --hstore он лежит в tags.
        p.tags->'addr:street' AS addr_street
    FROM planet_osm_polygon p
    WHERE p.building IS NOT NULL
      AND p."addr:housenumber" IS NOT NULL
      AND NULLIF(p."addr:housenumber", '') IS NOT NULL
    ORDER BY p.osm_id
)
INSERT INTO address_objects (id, parent_id, osm_id, osm_type, name, type, full_path, geom, created_at, updated_at)
SELECT
    uuid_generate_v5('6ba7b810-9dad-11d1-80b4-00c04fd430c8'::uuid, 'building:' || bg.osm_id::text),
    COALESCE(s.id, c.id),
    bg.osm_id,
    CASE WHEN bg.osm_id < 0 THEN 'relation' ELSE 'way' END,
    ba.housenumber,
    'building',
    COALESCE(s.full_path, c.full_path, '') || ', ' || ba.housenumber,
    bg.centroid,
    now(),
    now()
FROM building_geom bg
JOIN building_attrs ba ON ba.osm_id = bg.osm_id
LEFT JOIN LATERAL (
    SELECT s.id, s.full_path, s.geom
    FROM address_objects s
    WHERE s.type = 'street'
      AND s.name = ba.addr_street
    ORDER BY s.geom <-> bg.centroid
    LIMIT 1
) s ON TRUE
LEFT JOIN LATERAL (
    SELECT c.id, c.full_path, c.geom
    FROM address_objects c
    WHERE c.type IN ('city', 'village', 'suburb')
    ORDER BY c.geom <-> bg.centroid
    LIMIT 1
) c ON TRUE
ON CONFLICT (osm_id, osm_type) DO UPDATE SET
    parent_id  = EXCLUDED.parent_id,
    name       = EXCLUDED.name,
    full_path  = EXCLUDED.full_path,
    geom       = EXCLUDED.geom,
    updated_at = now();

SELECT 'building' AS type_inserted, COUNT(*) AS total FROM address_objects WHERE type = 'building';

-- =====================================================================
-- Итоговая сводка
-- =====================================================================
\echo ''
\echo '=== ИТОГ: address_objects по типам ==='
SELECT type, COUNT(*) FROM address_objects GROUP BY type ORDER BY type;

-- =====================================================================
-- Представление для отладки
-- =====================================================================
CREATE OR REPLACE VIEW v_city_stats AS
SELECT
    c.name AS city,
    (SELECT COUNT(*) FROM address_objects s WHERE s.parent_id = c.id AND s.type = 'street') AS streets,
    (SELECT COUNT(*) FROM address_objects b
     WHERE b.parent_id IN (
        SELECT id FROM address_objects s WHERE s.parent_id = c.id AND s.type = 'street'
     ) AND b.type = 'building') AS buildings
FROM address_objects c
WHERE c.type IN ('city', 'village')
ORDER BY streets DESC NULLS LAST, buildings DESC NULLS LAST;

\echo ''
\echo '=== Топ-5 городов по числу улиц ==='
SELECT * FROM v_city_stats LIMIT 5;
