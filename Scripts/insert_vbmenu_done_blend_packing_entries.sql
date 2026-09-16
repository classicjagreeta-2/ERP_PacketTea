/* =====================================================================
   Add menu entries to CLASSICNEW.VBMENU_DONE for:
     1. Master Blend Entry  (MasterBlendEntryController -> Index)  - "Master Blend Sheet"
     2. Final Blend Entry   (FinalBlendEntryController  -> Index)  - "Tea Blending Against Master"
     3. Package Entry       (PackingController          -> Index)  - "Packing Entry From Blend"
   PROJECTNAME / MODULECODE = 'PT' (PacketTea)
   Oracle / SQL Developer syntax

   NOTE: 01023010T was previously in use by another row (SALES INVOICE,
   then under FN). That row has since moved off this code (now
   01030020T under PT), so 01023010T / 01023020T / 01024010T were
   re-verified as free at the time this script was written. Step 1
   below re-checks this immediately before you run the insert, since
   this is a shared/live database - if it returns any rows, STOP and
   do not proceed with step 2.
   ===================================================================== */

-- 1) Preview: confirm these codes are still free before inserting.
SELECT ID, PROJECTNAME, MENUNAME, MENUCODE, CONTROLLER, ACTIONMETHOD, MODULECODE, ISACTIVE
FROM   CLASSICNEW.VBMENU_DONE
WHERE  MENUCODE IN ('01023010T', '01023020T', '01024010T');
-- Expect 0 rows. If this returns rows, STOP and pick different codes.

-- 2) Insert the three rows. IDs are derived from the current MAX(ID) so the
--    script stays correct even if more rows are added before it runs.
DECLARE
    v_id NUMBER;

    v_menucode_master CONSTANT VARCHAR2(20) := '01023010T';
    v_menucode_final  CONSTANT VARCHAR2(20) := '01023020T';
    v_menucode_pack   CONSTANT VARCHAR2(20) := '01024010T';
BEGIN
    SELECT NVL(MAX(ID), 0) INTO v_id FROM CLASSICNEW.VBMENU_DONE;

    INSERT INTO CLASSICNEW.VBMENU_DONE
        (ID, PROJECTNAME, MENUNAME, MENUCODE, CONTROLLER, ACTIONMETHOD, MODULECODE, IS4SUBDOMAIN, ISACTIVE)
    VALUES
        (v_id + 1, 'PT', 'Master Blend Entry', v_menucode_master, 'MasterBlendEntry', 'Index', 'PT', 1, 1);

    INSERT INTO CLASSICNEW.VBMENU_DONE
        (ID, PROJECTNAME, MENUNAME, MENUCODE, CONTROLLER, ACTIONMETHOD, MODULECODE, IS4SUBDOMAIN, ISACTIVE)
    VALUES
        (v_id + 2, 'PT', 'Final Blend Entry', v_menucode_final, 'FinalBlendEntry', 'Index', 'PT', 1, 1);

    INSERT INTO CLASSICNEW.VBMENU_DONE
        (ID, PROJECTNAME, MENUNAME, MENUCODE, CONTROLLER, ACTIONMETHOD, MODULECODE, IS4SUBDOMAIN, ISACTIVE)
    VALUES
        (v_id + 3, 'PT', 'Package Entry', v_menucode_pack, 'Packing', 'Index', 'PT', 1, 1);
END;
/

-- 3) Review the inserted rows.
SELECT ID, PROJECTNAME, MENUNAME, MENUCODE, CONTROLLER, ACTIONMETHOD, MODULECODE, ISACTIVE
FROM   CLASSICNEW.VBMENU_DONE
WHERE  MENUCODE IN ('01023010T', '01023020T', '01024010T')
ORDER  BY ID;

-- 4) Only after confirming it looks right:
COMMIT;
-- ROLLBACK;
