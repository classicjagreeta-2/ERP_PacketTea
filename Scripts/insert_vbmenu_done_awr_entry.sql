/* =====================================================================
   Enable the "AWR Entry" menu item for:
     AwrEntryController -> Index
   PROJECTNAME / MODULECODE = 'PT' (PacketTea)
   Oracle / SQL Developer syntax

   Confirmed from VBMENU_PACKET_TEA (the PT module's MENU_TABLE):
     ID=15, NAME='AWR Entry', ORDERCODE='01022010T', TYPE='E', PID='JAGUAR!2'
   Follows the same pattern as insert_vbmenu_done_blend_packing_entries.sql,
   which is already live for Master Blend Entry / Final Blend Entry / Packing Entry.
   ===================================================================== */

-- 1) Check whether a VBMENU_DONE row already exists for this menu code.
SELECT ID, PROJECTNAME, MENUNAME, MENUCODE, CONTROLLER, ACTIONMETHOD, MODULECODE, ISACTIVE
FROM   CLASSICNEW.VBMENU_DONE
WHERE  MENUCODE = '01022010T';

/* -----------------------------------------------------------------------
   CASE A: the query above returns 0 rows -> no mapping exists yet.
   Run steps 2A + 3.
   ----------------------------------------------------------------------- */

-- 2A) Insert the mapping.
DECLARE
    v_id NUMBER;
    v_menucode_awr CONSTANT VARCHAR2(20) := '01022010T';
BEGIN
    SELECT NVL(MAX(ID), 0) INTO v_id FROM CLASSICNEW.VBMENU_DONE;

    INSERT INTO CLASSICNEW.VBMENU_DONE
        (ID, PROJECTNAME, MENUNAME, MENUCODE, CONTROLLER, ACTIONMETHOD, MODULECODE, IS4SUBDOMAIN, ISACTIVE)
    VALUES
        (v_id + 1, 'PT', 'AWR Entry', v_menucode_awr, 'AwrEntry', 'Index', 'PT', 1, 1);
END;
/

/* -----------------------------------------------------------------------
   CASE B: the query in step 1 returns 1 row but ISACTIVE = 0 (or the
   CONTROLLER/ACTIONMETHOD values are wrong/blank) -> update instead of insert.
   Run this INSTEAD of step 2A, then skip to step 3.
   ----------------------------------------------------------------------- */
-- UPDATE CLASSICNEW.VBMENU_DONE
-- SET    CONTROLLER = 'AwrEntry',
--        ACTIONMETHOD = 'Index',
--        PROJECTNAME = 'PT',
--        MODULECODE = 'PT',
--        IS4SUBDOMAIN = 1,
--        ISACTIVE = 1
-- WHERE  MENUCODE = '01022010T';

-- 3) Review the row.
SELECT ID, PROJECTNAME, MENUNAME, MENUCODE, CONTROLLER, ACTIONMETHOD, MODULECODE, ISACTIVE
FROM   CLASSICNEW.VBMENU_DONE
WHERE  MENUCODE = '01022010T';

-- 4) Only after confirming it looks right:
COMMIT;
-- ROLLBACK;
