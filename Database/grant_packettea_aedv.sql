-- =====================================================================
-- PacketTea (module PT) -- grant AEDV rights on every screen
-- =====================================================================
-- Where AEDV lives (see ClassicERPCoreAPI MenuController):
--   FACT_JSTIL2027.USRACS_PACKET_TEA   one row per user (UID_ORA) per menu entry (MNUID):
--                                      AEDV letters + ADAY/EDAY/DDAY back-date days
--   CLASSICNEW.VBMENU_PACKET_TEA       the module's menu entries (ID, INDEX_ORA, ORDERCODE, PERDOTNETMENU)
--   CLASSICNEW.VBMENU_DONE             maps a menu entry's ORDERCODE (= MENUCODE) to the CONTROLLER
--                                      name the MVC app looks its permission row up by
--
-- HOW TO RUN (SQL Developer): open this file and press F5 ("Run Script"), or run each
-- statement on its own with Ctrl+Enter. There are no &variables in it -- every name is
-- written out -- so no substitution prompts appear. The connection you run it on must be able
-- to read CLASSICNEW.* and write FACT_JSTIL2027.USRACS_PACKET_TEA.
--
-- What it does
--   1. (read-only) shows which screens have a menu entry, and which don't.
--   2. For every user who holds a right on the shared "PacketTeaPurchaseEntry" screen
--      (the row Master/Final Blend, Packing etc. read today), inserts a row for each
--      target screen with AEDV = 'AEDV', copying that user's LOCA/UNIT/ADAY/EDAY/DDAY.
--      Existing rows are never duplicated.
--   3. (optional, commented out) widens rows that already exist but carry fewer letters.
--   4. verification query.
--
-- It does NOT COMMIT -- check the verification query, then COMMIT (or ROLLBACK).
--
-- Order of rollout: run this FIRST. The MVC controllers still read the shared
-- "PacketTeaPurchaseEntry" row today, so nothing changes for users until the controllers
-- are switched to each screen's own key.
--
-- Target screens (reports FullsStockActual/StockReport carry no AEDV logic and
-- PacketTeaInvoice already has its own key, so they are left out):
--   MasterBlendEntry, FinalBlendEntry, Packing, BlendEntry, PurchaseContractEntry, AWREntry,
--   TeaSampleDrawEntry, MarketReturn, ProductionEntry, OtherInvoice, PacketTeaNote,
--   TeaPurchaseNote
-- =====================================================================

-- ---------------------------------------------------------------------
-- 1. Which target screens have a menu entry? (read-only)
--    MENU_ID empty         => no menu entry / no VBMENU_DONE mapping: it needs a menu row
--                             first; the insert below skips it.
--    PERDOTNETMENU <> 'Y'  => the API ignores the entry when it builds a user's rights.
-- ---------------------------------------------------------------------
SELECT c.CONTROLLER,
       m.ID            AS MENU_ID,
       m.INDEX_ORA,
       m.NAME          AS MENU_NAME,
       m.ORDERCODE,
       m.PERDOTNETMENU,
       v.ISACTIVE
FROM   (SELECT column_value AS CONTROLLER
        FROM   TABLE(sys.odcivarchar2list(
                   'PacketTeaPurchaseEntry',
                   'MasterBlendEntry','FinalBlendEntry','Packing',
                   'BlendEntry','PurchaseContractEntry','AWREntry','TeaSampleDrawEntry',
                   'MarketReturn','ProductionEntry','OtherInvoice','PacketTeaNote','TeaPurchaseNote'))) c
LEFT JOIN CLASSICNEW.VBMENU_DONE       v ON v.CONTROLLER = c.CONTROLLER AND v.MODULECODE = 'PT'
LEFT JOIN CLASSICNEW.VBMENU_PACKET_TEA m ON m.ORDERCODE  = v.MENUCODE   AND m.MODULE     = 'PT'
ORDER  BY c.CONTROLLER;

-- ---------------------------------------------------------------------
-- 2. Grant: clone each PacketTeaPurchaseEntry holder's row onto every target screen
-- ---------------------------------------------------------------------
-- ID / AUTOID: if USRACS_PACKET_TEA fills them itself (trigger / identity / default) leave the
-- statement as it is. If the insert fails with ORA-01400 on ID or AUTOID, add them to the column
-- list and to the SELECT as "(SELECT NVL(MAX(ID),0) FROM FACT_JSTIL2027.USRACS_PACKET_TEA) + ROWNUM"
-- (same for AUTOID).
INSERT INTO FACT_JSTIL2027.USRACS_PACKET_TEA
       (INDEX_ORA, UID_ORA, MNUID, LOCA, UNIT, AEDV, EDAY, ADAY, DDAY, SCHEMA_NM,
        USER_NAME, USER_ENTDT)
SELECT t.INDEX_ORA,
       s.UID_ORA,
       t.ID,
       s.LOCA,
       s.UNIT,
       'AEDV',
       s.EDAY, s.ADAY, s.DDAY,
       s.SCHEMA_NM,
       USER,
       SYSDATE
FROM  -- source: users who hold a row on the shared PacketTeaPurchaseEntry screen
      (SELECT p.*
       FROM   FACT_JSTIL2027.USRACS_PACKET_TEA p
       JOIN   CLASSICNEW.VBMENU_PACKET_TEA sm ON sm.ID = p.MNUID AND sm.INDEX_ORA = p.INDEX_ORA AND sm.MODULE = 'PT'
       JOIN   CLASSICNEW.VBMENU_DONE sv       ON sv.MENUCODE = sm.ORDERCODE AND sv.MODULECODE = 'PT'
       WHERE  sv.CONTROLLER = 'PacketTeaPurchaseEntry') s
CROSS JOIN
      -- targets: each screen's menu entry
      (SELECT m.ID, m.INDEX_ORA
       FROM   CLASSICNEW.VBMENU_PACKET_TEA m
       JOIN   CLASSICNEW.VBMENU_DONE v ON v.MENUCODE = m.ORDERCODE AND v.MODULECODE = 'PT'
       WHERE  m.MODULE = 'PT'
       AND    v.CONTROLLER IN ('MasterBlendEntry','FinalBlendEntry','Packing',
                               'BlendEntry','PurchaseContractEntry','AWREntry','TeaSampleDrawEntry',
                               'MarketReturn','ProductionEntry','OtherInvoice','PacketTeaNote','TeaPurchaseNote')) t
WHERE NOT EXISTS (SELECT 1
                  FROM   FACT_JSTIL2027.USRACS_PACKET_TEA x
                  WHERE  x.UID_ORA   = s.UID_ORA
                  AND    x.MNUID     = t.ID
                  AND    x.INDEX_ORA = t.INDEX_ORA);

-- ---------------------------------------------------------------------
-- 3. OPTIONAL -- widen rows that already exist for these screens but hold fewer letters
--    Remove the comment markers to run. Days (ADAY/EDAY/DDAY) are left as they are.
-- ---------------------------------------------------------------------
-- UPDATE FACT_JSTIL2027.USRACS_PACKET_TEA x
-- SET    x.AEDV = 'AEDV'
-- WHERE  (x.MNUID, x.INDEX_ORA) IN
--        (SELECT m.ID, m.INDEX_ORA
--         FROM   CLASSICNEW.VBMENU_PACKET_TEA m
--         JOIN   CLASSICNEW.VBMENU_DONE v ON v.MENUCODE = m.ORDERCODE AND v.MODULECODE = 'PT'
--         WHERE  m.MODULE = 'PT'
--         AND    v.CONTROLLER IN ('MasterBlendEntry','FinalBlendEntry','Packing',
--                                 'BlendEntry','PurchaseContractEntry','AWREntry','TeaSampleDrawEntry',
--                                 'MarketReturn','ProductionEntry','OtherInvoice','PacketTeaNote','TeaPurchaseNote'))
-- AND    NVL(x.AEDV,'-') <> 'AEDV';

-- ---------------------------------------------------------------------
-- 4. Verify -- one row per user per screen. Every target screen should show AEDV
--    for every user who has PacketTeaPurchaseEntry rights.
-- ---------------------------------------------------------------------
SELECT p.UID_ORA, v.CONTROLLER, p.AEDV, p.ADAY, p.EDAY, p.DDAY, p.UNIT
FROM   FACT_JSTIL2027.USRACS_PACKET_TEA p
JOIN   CLASSICNEW.VBMENU_PACKET_TEA m ON m.ID = p.MNUID AND m.INDEX_ORA = p.INDEX_ORA AND m.MODULE = 'PT'
JOIN   CLASSICNEW.VBMENU_DONE v       ON v.MENUCODE = m.ORDERCODE AND v.MODULECODE = 'PT'
WHERE  v.CONTROLLER IN ('PacketTeaPurchaseEntry','MasterBlendEntry','FinalBlendEntry','Packing',
                        'BlendEntry','PurchaseContractEntry','AWREntry','TeaSampleDrawEntry',
                        'MarketReturn','ProductionEntry','OtherInvoice','PacketTeaNote','TeaPurchaseNote')
ORDER  BY p.UID_ORA, v.CONTROLLER;

-- Happy with the result?   COMMIT;      Not?   ROLLBACK;
