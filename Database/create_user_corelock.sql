-- =====================================================================
-- New PacketTea (PT) user CORELOCK -- a clone of CORETEAM's PT access
-- =====================================================================
-- Database: CLASSICDEV. Run as a user that can write CLASSICNEW.USER_JAG,
-- CLASSICNEW.USER_SCHEMA_LINK, FACT_JSTIL2027.USRACS_PACKET_TEA and
-- FACT_JSTIL2027.M_UNIT_USER_RIGHT (SQL Developer: F5 "Run Script").
--
-- BEFORE RUNNING: set the password. The scratchpad "pwenc" tool fills it in:
--   dotnet run -- CORELOCK <password> --selftest --into <this file>
-- (or paste the CHR(..) || CHR(..) ... text from
-- ClassicERPCore.API.Helpers.AccessStr.EncodeAsSql("CORELOCK", "<password>") over the
-- ENCODED_PASSWORD placeholder in step 1 by hand).
-- The value depends on the user name, so it must be generated for CORELOCK.
-- (Until you do, step 1 fails with a syntax error -- nothing is created half-way.)
--
-- What CORELOCK gets (all copied from CORETEAM, only the user name changed):
--   1. CLASSICNEW.USER_JAG               the login row (ID comes from USER_JAG_SEQ via trigger)
--   2. CLASSICNEW.USER_SCHEMA_LINK       PT company FACT_JSTIL2027, unit JSTI
--   3. FACT_JSTIL2027.USRACS_PACKET_TEA  every PT menu entry CORETEAM has, AEDV = full rights
--   4. FACT_JSTIL2027.M_UNIT_USER_RIGHT  packet types PT, ST, TT
-- It does NOT COMMIT: check the step-5 queries, then COMMIT (or ROLLBACK).
-- After committing, log in as CORELOCK (user name in capitals).
-- =====================================================================

-- 1. Login row. ID is filled by trigger USER_JAG_BIR. Mail settings are not copied.
INSERT INTO CLASSICNEW.USER_JAG
       (LOCA, USER_ORA, PASSWORD, ACCESS_STR, P1, USER_ENT, ENTDT, TIME_ORA, DTAG,
        GRP, USER_NAME, USER_PRIV, TIMESLOGGEDON, TIMESPWDCHANGED, PWDCYCLES,
        USERCREATED, ACTIVE_TAG, REMARKS)
SELECT c.LOCA, 'CORELOCK', NULL,
       CHR(148) || CHR(132) || CHR(106) || CHR(164) || CHR(120) || CHR(91) || CHR(46) || CHR(118) || CHR(84) || CHR(56) || SUBSTR(c.ACCESS_STR, 11),
       c.P1, USER, SYSDATE, TO_CHAR(SYSDATE, 'HH24:MI'), NULL,
       c.GRP, 'CORE LOCK TEST', c.USER_PRIV, 0, 0, NVL(c.PWDCYCLES, 0),
       SYSDATE, c.ACTIVE_TAG, 'Edit-lock test user (clone of CORETEAM)'
FROM   CLASSICNEW.USER_JAG c
WHERE  c.USER_ORA = 'CORETEAM'
AND    NOT EXISTS (SELECT 1 FROM CLASSICNEW.USER_JAG x WHERE x.USER_ORA = 'CORELOCK');

-- 2. PT company / unit link (the Unit lists on every PT screen come from here).
--    ID is a GENERATED ALWAYS identity column -- Oracle fills it, it must not be listed.
INSERT INTO CLASSICNEW.USER_SCHEMA_LINK
       (LOCA, UNIT, USER_ORA, USR_ID, MODULE_NAME, USER_ENT, ENTDT, TIME_ORA,
        DTAG, API_ALLOW, MODULE_CODE, TECHNOLOGY)
SELECT c.LOCA, c.UNIT, 'CORELOCK', c.USR_ID, c.MODULE_NAME, USER, SYSDATE, TO_CHAR(SYSDATE, 'HH24:MI'),
       c.DTAG, c.API_ALLOW, c.MODULE_CODE, c.TECHNOLOGY
FROM   CLASSICNEW.USER_SCHEMA_LINK c
WHERE  c.USER_ORA = 'CORETEAM'
AND    c.MODULE_CODE = 'PT'
AND    NOT EXISTS (SELECT 1 FROM CLASSICNEW.USER_SCHEMA_LINK x
                   WHERE x.USER_ORA = 'CORELOCK' AND x.MODULE_CODE = 'PT' AND x.USR_ID = c.USR_ID);

-- 3. Menu rights: one row per menu entry CORETEAM has (group headers included -- the
--    menu tree is built only from rows the user holds), all with full AEDV rights and
--    CORETEAM's back-date days.
INSERT INTO FACT_JSTIL2027.USRACS_PACKET_TEA
       (ID, LOCA, UNIT, UID_ORA, MNUID, INDEX_ORA, AEDV, EDAY, ADAY, DDAY,
        USER_ORA, ENTDT, TIME_ORA, USER_NAME, USER_ENTDT, OS_USER, TERMINAL_ID, DTAG, SCHEMA_NM)
SELECT (SELECT NVL(MAX(ID), 0) FROM FACT_JSTIL2027.USRACS_PACKET_TEA) + ROWNUM,
       c.LOCA, c.UNIT, 'CORELOCK', c.MNUID, c.INDEX_ORA, 'AEDV',
       NVL(c.EDAY, 0), NVL(c.ADAY, 0), NVL(c.DDAY, 0),
       USER, SYSDATE, TO_CHAR(SYSDATE, 'HH24:MI'), SUBSTR(USER, 1, 10), SYSDATE,
       c.OS_USER, c.TERMINAL_ID, c.DTAG, c.SCHEMA_NM
FROM   FACT_JSTIL2027.USRACS_PACKET_TEA c
WHERE  c.UID_ORA = 'CORETEAM'
AND    NOT EXISTS (SELECT 1 FROM FACT_JSTIL2027.USRACS_PACKET_TEA x
                   WHERE x.UID_ORA = 'CORELOCK' AND x.MNUID = c.MNUID AND x.INDEX_ORA = c.INDEX_ORA);

-- 4. Packet types (the Blend / Packing / AWR / Sample Draw / Tea Purchase Note type lists).
INSERT INTO FACT_JSTIL2027.M_UNIT_USER_RIGHT
       (ID, LOCA, UID_ORA, TYPE, ADAY, DDAY, EDAY, USER_NAME, USER_ENTDT, OS_USER, TERMINAL_ID, DTAG)
SELECT (SELECT NVL(MAX(ID), 0) FROM FACT_JSTIL2027.M_UNIT_USER_RIGHT) + ROWNUM,
       c.LOCA, 'CORELOCK', c.TYPE, c.ADAY, c.DDAY, c.EDAY, SUBSTR(USER, 1, 10), SYSDATE,
       c.OS_USER, c.TERMINAL_ID, c.DTAG
FROM   FACT_JSTIL2027.M_UNIT_USER_RIGHT c
WHERE  c.UID_ORA = 'CORETEAM'
AND    NOT EXISTS (SELECT 1 FROM FACT_JSTIL2027.M_UNIT_USER_RIGHT x
                   WHERE x.UID_ORA = 'CORELOCK' AND x.TYPE = c.TYPE);

-- 5. Check (expected: 1 / 1 / same count as CORETEAM / 3), then COMMIT.
SELECT 'USER_JAG' t, COUNT(*) n FROM CLASSICNEW.USER_JAG WHERE USER_ORA = 'CORELOCK'
UNION ALL SELECT 'USER_SCHEMA_LINK', COUNT(*) FROM CLASSICNEW.USER_SCHEMA_LINK WHERE USER_ORA = 'CORELOCK' AND MODULE_CODE = 'PT'
UNION ALL SELECT 'USRACS_PACKET_TEA', COUNT(*) FROM FACT_JSTIL2027.USRACS_PACKET_TEA WHERE UID_ORA = 'CORELOCK'
UNION ALL SELECT 'USRACS (CORETEAM)', COUNT(*) FROM FACT_JSTIL2027.USRACS_PACKET_TEA WHERE UID_ORA = 'CORETEAM'
UNION ALL SELECT 'M_UNIT_USER_RIGHT', COUNT(*) FROM FACT_JSTIL2027.M_UNIT_USER_RIGHT WHERE UID_ORA = 'CORELOCK';

-- COMMIT;
