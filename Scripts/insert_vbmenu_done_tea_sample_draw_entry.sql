/* =====================================================================
   Enable the "Tea Sample Draw Entry" menu item for:
     TeaSampleDrawEntryController -> Index
   PROJECTNAME / MODULECODE = 'PT' (PacketTea)
   Oracle / SQL Developer syntax

   Confirmed from VBMENU_PACKET_TEA (the PT module's MENU_TABLE), same
   result set that gave AWR Entry's row:
     ID=16, NAME='Tea Sample Draw Entry', ORDERCODE='01022020T', TYPE='E', PID='JAGUAR!2'
   Follows the same pattern as insert_vbmenu_done_awr_entry.sql (already
   run/committed and confirmed working).
   ===================================================================== */

-- 1) Check whether a VBMENU_DONE row already exists for this menu code.
SELECT ID, PROJECTNAME, MENUNAME, MENUCODE, CONTROLLER, ACTIONMETHOD, MODULECODE, ISACTIVE
FROM   CLASSICNEW.VBMENU_DONE
WHERE  MENUCODE = '01022020T';

/* -----------------------------------------------------------------------
   CASE A: 0 rows -> no mapping exists yet. Run 2A + 3.
   ----------------------------------------------------------------------- */

-- 2A) Insert the mapping.
DECLARE
    v_id NUMBER;
    v_menucode_sd CONSTANT VARCHAR2(20) := '01022020T';
BEGIN
    SELECT NVL(MAX(ID), 0) INTO v_id FROM CLASSICNEW.VBMENU_DONE;

    INSERT INTO CLASSICNEW.VBMENU_DONE
        (ID, PROJECTNAME, MENUNAME, MENUCODE, CONTROLLER, ACTIONMETHOD, MODULECODE, IS4SUBDOMAIN, ISACTIVE)
    VALUES
        (v_id + 1, 'PT', 'Tea Sample Draw Entry', v_menucode_sd, 'TeaSampleDrawEntry', 'Index', 'PT', 1, 1);
END;
/

/* -----------------------------------------------------------------------
   CASE B: a row exists but ISACTIVE = 0 (or CONTROLLER/ACTIONMETHOD are
   wrong/blank) -> update instead of insert. Run this INSTEAD of 2A.
   ----------------------------------------------------------------------- */
-- UPDATE CLASSICNEW.VBMENU_DONE
-- SET    CONTROLLER = 'TeaSampleDrawEntry',
--        ACTIONMETHOD = 'Index',
--        PROJECTNAME = 'PT',
--        MODULECODE = 'PT',
--        IS4SUBDOMAIN = 1,
--        ISACTIVE = 1
-- WHERE  MENUCODE = '01022020T';

-- 3) Review the row.
SELECT ID, PROJECTNAME, MENUNAME, MENUCODE, CONTROLLER, ACTIONMETHOD, MODULECODE, ISACTIVE
FROM   CLASSICNEW.VBMENU_DONE
WHERE  MENUCODE = '01022020T';

-- 4) Only after confirming it looks right:
COMMIT;
-- ROLLBACK;
