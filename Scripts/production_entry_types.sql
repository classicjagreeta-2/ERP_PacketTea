-- =====================================================================
-- Production Entry: Type dropdown
-- The dropdown lists every packet-tea type (M_SALETYPE.TRN_TYPE = 'P')
-- that the user has a row for in M_UNIT_USER_RIGHT, both in the Sales
-- schema. Run in SQL Developer as a user that can write FIN_JSTIL2027.
-- Change CORETEAM / JST below for other users or locations.
-- =====================================================================

-- 1. Which packet-tea types exist in the Sales schema?
SELECT TRIM(CODE) CODE, DESCN, UNIT, LOCA
FROM   FIN_JSTIL2027.M_SALETYPE
WHERE  TRN_TYPE = 'P'
ORDER  BY CODE;

-- 1a. If step 1 errors with ORA-00942 (table missing) or returns no rows,
--     copy the packet-tea types from the operating schema first
--     (only when both M_SALETYPE tables have identical columns).
-- CREATE TABLE FIN_JSTIL2027.M_SALETYPE AS
--     SELECT * FROM FACT_JSTIL2027.M_SALETYPE WHERE 1 = 0;      -- only if the table is missing
-- INSERT INTO FIN_JSTIL2027.M_SALETYPE
--     SELECT * FROM FACT_JSTIL2027.M_SALETYPE WHERE TRN_TYPE = 'P';
-- COMMIT;

-- 2. Which types does the user already hold?
SELECT LOCA, UID_ORA, TYPE
FROM   FIN_JSTIL2027.M_UNIT_USER_RIGHT
WHERE  UID_ORA = 'CORETEAM'
ORDER  BY TYPE;

-- 3a. Grant ALL packet-tea types to the user (skips ones already granted).
INSERT INTO FIN_JSTIL2027.M_UNIT_USER_RIGHT (LOCA, UID_ORA, TYPE)
SELECT 'JST', 'CORETEAM', st.CODE
FROM  (SELECT DISTINCT TRIM(CODE) CODE
       FROM   FIN_JSTIL2027.M_SALETYPE
       WHERE  TRN_TYPE = 'P') st
WHERE NOT EXISTS (SELECT 1
                  FROM   FIN_JSTIL2027.M_UNIT_USER_RIGHT r
                  WHERE  r.UID_ORA = 'CORETEAM'
                  AND    TRIM(r.TYPE) = st.CODE);

-- 3b. ...or grant only specific types instead of 3a (edit the list).
-- INSERT INTO FIN_JSTIL2027.M_UNIT_USER_RIGHT (LOCA, UID_ORA, TYPE)
-- SELECT 'JST', 'CORETEAM', t.CODE
-- FROM  (SELECT 'TB' CODE FROM DUAL UNION ALL
--        SELECT 'BT'      FROM DUAL) t
-- WHERE NOT EXISTS (SELECT 1 FROM FIN_JSTIL2027.M_UNIT_USER_RIGHT r
--                   WHERE r.UID_ORA = 'CORETEAM' AND TRIM(r.TYPE) = t.CODE);

COMMIT;

-- 4. Verify: exactly what the Type dropdown will show.
SELECT TRIM(st.CODE) CODE, MAX(st.DESCN) DESCN
FROM   FIN_JSTIL2027.M_SALETYPE st
JOIN   FIN_JSTIL2027.M_UNIT_USER_RIGHT r ON TRIM(r.TYPE) = TRIM(st.CODE)
WHERE  st.TRN_TYPE = 'P'
AND    r.UID_ORA = 'CORETEAM'
GROUP  BY TRIM(st.CODE)
ORDER  BY 1;
