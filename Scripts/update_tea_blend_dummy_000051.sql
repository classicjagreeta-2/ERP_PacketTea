/* =====================================================================
   Dummy data update for FACT_JSTIL2027.T_TEA_BLEND, DOCNO = '000051'
   Fields: Export Ref No, Transporter, Ship Mark, Notes, Special Instructions
   Oracle / SQL Developer syntax
   ===================================================================== */

-- 1) Preview the row(s) this will touch before updating.
SELECT ID, LOCA, GLOCA, UNIT, DOCNO, DOCDT, BLEND_TYPE,
       EXPREF_NO, TPT, SHIP_MARK, NOTES, SP_NOTES
FROM   FACT_JSTIL2027.T_TEA_BLEND
WHERE  DOCNO = '000051';

-- 2) Apply dummy values.
--    TPT is a transporter CODE (see GetTransporter lookup in MasterBlendEntryController) -
--    replace 'TR001' below with a valid code from your transporter master if this
--    placeholder does not exist in FACT_JSTIL2027.
UPDATE FACT_JSTIL2027.T_TEA_BLEND
SET    EXPREF_NO = 'EXP-REF-000051',
       TPT       = 'TR001',
       SHIP_MARK = 'SHIP-MARK-DEMO',
       NOTES     = 'Dummy notes for docno 000051 - test data',
       SP_NOTES  = 'Dummy special instructions for docno 000051 - test data'
WHERE  DOCNO = '000051';

-- 3) Check the rows-affected count SQL Developer reports for the UPDATE above,
--    then review the row again:
SELECT ID, LOCA, GLOCA, UNIT, DOCNO, DOCDT, BLEND_TYPE,
       EXPREF_NO, TPT, SHIP_MARK, NOTES, SP_NOTES
FROM   FACT_JSTIL2027.T_TEA_BLEND
WHERE  DOCNO = '000051';

-- 4) Only after confirming it looks right:
COMMIT;
-- ROLLBACK;
